using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Dtos;
using RESTfulAPIPWeb.Entities;
using RESTfulAPIPWeb.Repositories.Interfaces;
using System.Security.Claims;

namespace RESTfulAPIPWeb.Controllers
{
    /// <summary>
    /// Controller para gestão de produtos da API MyMEDIA
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
            
            // Utilizadores anónimos e clientes só veem produtos ativos
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
        /// Lista TODOS os produtos (incluindo pendentes e inativos) - só para Admin/Funcionário
        /// </summary>
        [HttpGet("todos")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetTodosProdutos()
        {
            var produtos = await _repo.GetAllAsync();

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
        /// Lista produtos por categoria
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
        /// Obtém produto em destaque (aleatório)
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
        /// Obtém um produto pelo ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProdutoDto>> GetProduto(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            if (p == null) return NotFound();

            // Utilizadores anónimos só veem produtos ativos
            if (p.Estado != "Ativo" && !User.Identity?.IsAuthenticated == true)
            {
                // Verificar se é admin/funcionário
                if (!User.IsInRole("Administrador") && !User.IsInRole("Funcionário"))
                    return NotFound();
            }

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
        /// Cria um novo produto (Admin/Funcionário ou Fornecedor para os seus próprios)
        /// Produtos criados ficam no estado "Pendente"
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrador,Funcionário,Fornecedor")]
        public async Task<ActionResult<ProdutoDto>> CreateProduto([FromBody] ProdutoCreateDto dto)
        {
            // Se for fornecedor, só pode criar produtos para si próprio
            if (User.IsInRole("Fornecedor"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var fornecedor = await _context.Fornecedores
                    .FirstOrDefaultAsync(f => f.ApplicationUserId == userId);

                if (fornecedor == null)
                    return Unauthorized(new { Message = "Fornecedor não encontrado." });

                dto.FornecedorId = fornecedor.Id;
                dto.Estado = "Pendente"; // Fornecedor não pode ativar diretamente
            }

            var produto = new Produto
            {
                Nome = dto.Nome,
                PrecoBase = dto.PrecoBase,
                Percentagem = dto.Percentagem,
                PrecoFinal = dto.PrecoBase * (1 + dto.Percentagem / 100),
                Estado = dto.Estado,
                Stock = dto.Stock,
                Imagem = dto.Imagem,
                CategoriaId = dto.CategoriaId,
                ModoEntregaId = dto.ModoEntregaId,
                FornecedorId = dto.FornecedorId
            };

            var created = await _repo.AddAsync(produto);

            return CreatedAtAction(nameof(GetProduto), new { id = created.Id }, new ProdutoDto
            {
                Id = created.Id,
                Nome = created.Nome,
                PrecoBase = created.PrecoBase,
                Percentagem = created.Percentagem,
                PrecoFinal = created.PrecoFinal,
                Estado = created.Estado,
                Stock = created.Stock,
                Imagem = created.Imagem,
                CategoriaId = created.CategoriaId,
                ModoEntregaId = created.ModoEntregaId,
                FornecedorId = created.FornecedorId
            });
        }

        /// <summary>
        /// Atualiza um produto existente
        /// Fornecedores só podem editar os seus próprios produtos
        /// Após edição, produto volta ao estado "Pendente"
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Funcionário,Fornecedor")]
        public async Task<IActionResult> UpdateProduto(int id, [FromBody] ProdutoUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var produtoExistente = await _repo.GetByIdAsync(id);
            if (produtoExistente == null) return NotFound();

            // Se for fornecedor, só pode editar os seus próprios produtos
            if (User.IsInRole("Fornecedor") && !User.IsInRole("Administrador") && !User.IsInRole("Funcionário"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var fornecedor = await _context.Fornecedores
                    .FirstOrDefaultAsync(f => f.ApplicationUserId == userId);

                if (fornecedor == null || produtoExistente.FornecedorId != fornecedor.Id)
                    return Forbid();

                dto.Estado = "Pendente"; // Após edição, volta para pendente
            }

            var produto = new Produto
            {
                Id = dto.Id,
                Nome = dto.Nome,
                PrecoBase = dto.PrecoBase,
                Percentagem = dto.Percentagem,
                PrecoFinal = dto.PrecoBase * (1 + dto.Percentagem / 100),
                Estado = dto.Estado,
                Stock = dto.Stock,
                Imagem = dto.Imagem,
                CategoriaId = dto.CategoriaId,
                ModoEntregaId = dto.ModoEntregaId,
                FornecedorId = dto.FornecedorId
            };

            var ok = await _repo.UpdateAsync(produto);
            if (!ok) return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Ativa um produto (só Admin/Funcionário)
        /// Aplica a percentagem e calcula o preço final
        /// </summary>
        [HttpPut("{id}/ativar")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> AtivarProduto(int id, [FromBody] decimal? percentagem = null)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound();

            if (percentagem.HasValue)
            {
                produto.Percentagem = percentagem.Value;
                produto.PrecoFinal = produto.PrecoBase * (1 + percentagem.Value / 100);
            }

            produto.Estado = "Ativo";
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Produto ativado com sucesso.", PrecoFinal = produto.PrecoFinal });
        }

        /// <summary>
        /// Inativa um produto (só Admin/Funcionário)
        /// </summary>
        [HttpPut("{id}/inativar")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> InativarProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound();

            produto.Estado = "Inativo";
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Produto inativado com sucesso." });
        }

        /// <summary>
        /// Apaga um produto (só se não tiver vendas associadas)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            // Verificar se existem vendas associadas
            var temVendas = await _context.LinhasVenda.AnyAsync(l => l.ProdutoId == id);
            if (temVendas)
                return BadRequest(new { Message = "Não é possível apagar este produto porque existem vendas associadas." });

            var ok = await _repo.DeleteAsync(id);
            if (!ok) return NotFound();

            return NoContent();
        }
    }
}
