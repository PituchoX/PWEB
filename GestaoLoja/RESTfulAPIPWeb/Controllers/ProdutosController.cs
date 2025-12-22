using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Dtos;
using RESTfulAPIPWeb.Repositories.Interfaces;

namespace RESTfulAPIPWeb.Controllers
{
    /// <summary>
    /// Controller para consulta de produtos da API MyMEDIA
    /// Apenas produtos ATIVOS são visíveis para utilizadores anónimos e clientes
    /// Admin/Funcionário gerem produtos na aplicação GestaoLoja
    /// Fornecedores gerem os seus produtos via FornecedorProdutosController
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoRepository _repo;
        private readonly AppDbContext _context;

        public ProdutosController(IProdutoRepository repo, AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        /// <summary>
        /// Lista todos os produtos ATIVOS (visíveis para utilizadores anónimos e clientes)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetProdutos()
        {
            var produtos = await _repo.GetAllAsync();
            
            // Apenas produtos ativos são visíveis
            var produtosVisiveis = produtos.Where(p => p.Estado == "Ativo");

            var result = produtosVisiveis.Select(p => new ProdutoDto
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
                CategoriaNome = p.Categoria?.Nome,
                ModoEntregaId = p.ModoEntregaId,
                ModoEntregaNome = p.ModoEntrega?.Nome,
                FornecedorId = p.FornecedorId,
                FornecedorNome = p.Fornecedor?.NomeEmpresa
            });

            return Ok(result);
        }

        /// <summary>
        /// Lista produtos por categoria (apenas ativos)
        /// </summary>
        [HttpGet("categoria/{categoriaId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetProdutosPorCategoria(int categoriaId)
        {
            var produtos = await _context.Produtos
                .Where(p => p.CategoriaId == categoriaId && p.Estado == "Ativo")
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .Include(p => p.Fornecedor)
                .ToListAsync();

            var result = produtos.Select(p => new ProdutoDto
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
                CategoriaNome = p.Categoria?.Nome,
                ModoEntregaId = p.ModoEntregaId,
                ModoEntregaNome = p.ModoEntrega?.Nome,
                FornecedorId = p.FornecedorId,
                FornecedorNome = p.Fornecedor?.NomeEmpresa
            });

            return Ok(result);
        }

        /// <summary>
        /// Obtém produto em destaque (aleatório entre os ativos com stock)
        /// </summary>
        [HttpGet("destaque")]
        [AllowAnonymous]
        public async Task<ActionResult<ProdutoDto>> GetProdutoDestaque()
        {
            var produtos = await _context.Produtos
                .Where(p => p.Estado == "Ativo" && p.Stock > 0)
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .Include(p => p.Fornecedor)
                .ToListAsync();

            if (!produtos.Any())
                return NotFound(new { Message = "Nenhum produto disponível." });

            var random = new Random();
            var p = produtos[random.Next(produtos.Count)];

            return Ok(new ProdutoDto
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
                CategoriaNome = p.Categoria?.Nome,
                ModoEntregaId = p.ModoEntregaId,
                ModoEntregaNome = p.ModoEntrega?.Nome,
                FornecedorId = p.FornecedorId,
                FornecedorNome = p.Fornecedor?.NomeEmpresa
            });
        }

        /// <summary>
        /// Obtém um produto pelo ID (apenas se estiver ativo)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProdutoDto>> GetProduto(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            if (p == null) return NotFound();

            // Apenas produtos ativos são visíveis
            if (p.Estado != "Ativo")
                return NotFound();

            return Ok(new ProdutoDto
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
                CategoriaNome = p.Categoria?.Nome,
                ModoEntregaId = p.ModoEntregaId,
                ModoEntregaNome = p.ModoEntrega?.Nome,
                FornecedorId = p.FornecedorId,
                FornecedorNome = p.Fornecedor?.NomeEmpresa
            });
        }
    }
}
