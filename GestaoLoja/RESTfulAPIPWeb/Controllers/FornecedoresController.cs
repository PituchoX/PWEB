using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Dtos;

namespace RESTfulAPIPWeb.Controllers
{
    /// <summary>
    /// Controller para consulta de fornecedores da API MyMEDIA
    /// Admin/Funcionário gerem fornecedores na aplicação GestaoLoja
    /// </summary>
    [Authorize(Roles = "Administrador,Funcionário")]
    [Route("api/[controller]")]
    [ApiController]
    public class FornecedoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FornecedoresController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista todos os fornecedores
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FornecedorDto>>> GetFornecedores()
        {
            var fornecedores = await _context.Fornecedores
                .Include(f => f.ApplicationUser)
                .ToListAsync();

            var result = fornecedores.Select(f => new FornecedorDto
            {
                Id = f.Id,
                ApplicationUserId = f.ApplicationUserId,
                Nome = f.ApplicationUser?.NomeCompleto ?? "",
                Email = f.ApplicationUser?.Email ?? "",
                NomeEmpresa = f.NomeEmpresa,
                NIF = f.NIF,
                Estado = f.ApplicationUser?.Estado ?? f.Estado
            });

            return Ok(result);
        }

        /// <summary>
        /// Obtém um fornecedor pelo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<FornecedorDto>> GetFornecedor(string id)
        {
            var fornecedor = await _context.Fornecedores
                .Include(f => f.ApplicationUser)
                .FirstOrDefaultAsync(f => f.ApplicationUserId == id);

            if (fornecedor == null)
                return NotFound();

            return Ok(new FornecedorDto
            {
                Id = fornecedor.Id,
                ApplicationUserId = fornecedor.ApplicationUserId,
                Nome = fornecedor.ApplicationUser?.NomeCompleto ?? "",
                Email = fornecedor.ApplicationUser?.Email ?? "",
                NomeEmpresa = fornecedor.NomeEmpresa,
                NIF = fornecedor.NIF,
                Estado = fornecedor.ApplicationUser?.Estado ?? fornecedor.Estado
            });
        }

        /// <summary>
        /// Lista produtos de um fornecedor específico
        /// </summary>
        [HttpGet("{id}/produtos")]
        public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetProdutosDoFornecedor(string id)
        {
            var fornecedor = await _context.Fornecedores
                .FirstOrDefaultAsync(f => f.ApplicationUserId == id);

            if (fornecedor == null)
                return NotFound("Fornecedor não encontrado.");

            var produtos = await _context.Produtos
                .Where(p => p.FornecedorId == fornecedor.Id)
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .Select(p => new ProdutoDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    PrecoBase = p.PrecoBase,
                    Percentagem = p.Percentagem,
                    PrecoFinal = p.PrecoFinal,
                    Estado = p.Estado,
                    Stock = p.Stock,
                    Imagem = p.Imagem,
                    CategoriaId = p.CategoriaId,
                    CategoriaNome = p.Categoria != null ? p.Categoria.Nome : null,
                    ModoEntregaId = p.ModoEntregaId,
                    ModoEntregaNome = p.ModoEntrega != null ? p.ModoEntrega.Nome : null,
                    FornecedorId = p.FornecedorId,
                    FornecedorNome = fornecedor.NomeEmpresa
                })
                .ToListAsync();

            return Ok(produtos);
        }
    }
}
