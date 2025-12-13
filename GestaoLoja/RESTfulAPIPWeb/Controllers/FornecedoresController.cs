using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Dtos;
using RESTfulAPIPWeb.Entities;

namespace RESTfulAPIPWeb.Controllers
{
    [Authorize(Roles = "Administrador,Funcionário")]
    [Route("api/[controller]")]
    [ApiController]
    public class FornecedoresController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FornecedoresController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ================================================================
        // GET: api/fornecedores
        // Lista todos os fornecedores
        // ================================================================
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

        // ================================================================
        // GET: api/fornecedores/{id}
        // ================================================================
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

        // ================================================================
        // PUT: api/fornecedores/{id}/estado
        // Alterar estado: Pendente / Ativo / Inativo
        // ================================================================
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> AtualizarEstado(string id, [FromBody] FornecedorEstadoUpdateDto dto)
        {
            var validStates = new[] { "Pendente", "Ativo", "Inativo" };

            if (!validStates.Contains(dto.NovoEstado))
                return BadRequest("Estado inválido. Utilize: Pendente, Ativo ou Inativo.");

            var fornecedor = await _context.Fornecedores
                .Include(f => f.ApplicationUser)
                .FirstOrDefaultAsync(f => f.ApplicationUserId == id);

            if (fornecedor == null)
                return NotFound();

            fornecedor.ApplicationUser!.Estado = dto.NovoEstado;
            fornecedor.Estado = dto.NovoEstado;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Estado atualizado com sucesso." });
        }

        // ================================================================
        // GET: api/fornecedores/{id}/produtos
        // Lista produtos pertencentes ao fornecedor
        // ================================================================
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
