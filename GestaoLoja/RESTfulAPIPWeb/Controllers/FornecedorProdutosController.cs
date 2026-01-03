using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Dtos;
using RESTfulAPIPWeb.Entities;
using System.Security.Claims;

namespace RESTfulAPIPWeb.Controllers
{
    [Route("api/fornecedor/produtos")]
    [ApiController]
    [Authorize(Roles = "Fornecedor")]
    public class FornecedorProdutosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FornecedorProdutosController> _logger;

        public FornecedorProdutosController(AppDbContext context, ILogger<FornecedorProdutosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtém o fornecedor do utilizador atual - BUSCA POR EMAIL para garantir consistência
        /// </summary>
        private async Task<Fornecedor?> GetFornecedorAtual()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value 
                         ?? User.FindFirst("email")?.Value
                         ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
            
            _logger.LogInformation($"GetFornecedorAtual - UserId: {userId}, Email: {userEmail}");
            
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(userEmail)) 
                return null;

            Fornecedor? fornecedor = null;

            // PRIORIDADE 1: Buscar pelo EMAIL (mais confiável entre diferentes sistemas)
            if (!string.IsNullOrEmpty(userEmail))
            {
                fornecedor = await _context.Fornecedores
                    .Include(f => f.ApplicationUser)
                    .FirstOrDefaultAsync(f => f.ApplicationUser != null && 
                                              f.ApplicationUser.Email == userEmail);
                
                if (fornecedor != null)
                {
                    _logger.LogInformation($"Fornecedor encontrado pelo email: Id={fornecedor.Id}, Nome={fornecedor.NomeEmpresa}");
                    return fornecedor;
                }
            }

            // PRIORIDADE 2: Buscar pelo ApplicationUserId
            if (!string.IsNullOrEmpty(userId))
            {
                fornecedor = await _context.Fornecedores
                    .FirstOrDefaultAsync(f => f.ApplicationUserId == userId);
                
                if (fornecedor != null)
                {
                    _logger.LogInformation($"Fornecedor encontrado pelo UserId: Id={fornecedor.Id}");
                    return fornecedor;
                }
            }

            // PRIORIDADE 3: Se não encontrou por nenhum, criar automaticamente
            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.Id == userId || u.Email == userEmail);
            
            if (user != null && user.Perfil == "Fornecedor")
            {
                _logger.LogInformation($"A criar fornecedor automaticamente para: {user.Email}");
                
                fornecedor = new Fornecedor
                {
                    ApplicationUserId = user.Id,
                    NomeEmpresa = user.NomeCompleto + " (Empresa)",
                    Estado = "Aprovado"
                };
                _context.Fornecedores.Add(fornecedor);
                await _context.SaveChangesAsync();
                
                return fornecedor;
            }

            _logger.LogWarning($"Fornecedor NÃO encontrado para userId: {userId}, email: {userEmail}");
            return null;
        }

        /// <summary>
        /// Lista os produtos do fornecedor atual
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetMeusProdutos()
        {
            var fornecedor = await GetFornecedorAtual();
            if (fornecedor == null)
                return Ok(new List<ProdutoDto>()); // Lista vazia em vez de erro

            _logger.LogInformation($"A carregar produtos para FornecedorId={fornecedor.Id}");

            var produtos = await _context.Produtos
                .Where(p => p.FornecedorId == fornecedor.Id)
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            _logger.LogInformation($"Encontrados {produtos.Count} produtos");

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
                FornecedorNome = fornecedor.NomeEmpresa
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoDto>> GetMeuProduto(int id)
        {
            var fornecedor = await GetFornecedorAtual();
            if (fornecedor == null)
                return Unauthorized(new { Message = "Fornecedor não encontrado." });

            var p = await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .FirstOrDefaultAsync(p => p.Id == id && p.FornecedorId == fornecedor.Id);

            if (p == null)
                return NotFound(new { Message = "Produto não encontrado." });

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
                FornecedorNome = fornecedor.NomeEmpresa
            });
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoDto>> CriarProduto([FromBody] ProdutoCreateDto dto)
        {
            var fornecedor = await GetFornecedorAtual();
            if (fornecedor == null)
                return Unauthorized(new { Message = "Fornecedor não encontrado." });

            if (fornecedor.Estado != "Aprovado")
                return BadRequest(new { Message = "O seu registo ainda não foi aprovado." });

            var produto = new Produto
            {
                Nome = dto.Nome,
                PrecoBase = dto.PrecoBase,
                Percentagem = 0,
                PrecoFinal = dto.PrecoBase,
                Estado = "Pendente",
                Stock = dto.Stock,
                Imagem = string.IsNullOrEmpty(dto.Imagem) ? "semfoto.png" : dto.Imagem,
                CategoriaId = dto.CategoriaId,
                ModoEntregaId = dto.ModoEntregaId,
                FornecedorId = fornecedor.Id
            };

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeuProduto), new { id = produto.Id }, new ProdutoDto
            {
                Id = produto.Id,
                Nome = produto.Nome,
                PrecoBase = produto.PrecoBase,
                Percentagem = produto.Percentagem,
                PrecoFinal = produto.PrecoFinal,
                Estado = produto.Estado,
                Stock = produto.Stock,
                Imagem = produto.Imagem,
                CategoriaId = produto.CategoriaId,
                ModoEntregaId = produto.ModoEntregaId,
                FornecedorId = produto.FornecedorId,
                FornecedorNome = fornecedor.NomeEmpresa
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarProduto(int id, [FromBody] ProdutoUpdateDto dto)
        {
            var fornecedor = await GetFornecedorAtual();
            if (fornecedor == null)
                return Unauthorized(new { Message = "Fornecedor não encontrado." });

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.FornecedorId == fornecedor.Id);

            if (produto == null)
                return NotFound(new { Message = "Produto não encontrado." });

            produto.Nome = dto.Nome;
            produto.PrecoBase = dto.PrecoBase;
            produto.Stock = dto.Stock;
            produto.Imagem = string.IsNullOrEmpty(dto.Imagem) ? produto.Imagem : dto.Imagem;
            produto.CategoriaId = dto.CategoriaId;
            produto.ModoEntregaId = dto.ModoEntregaId;
            produto.Estado = "Pendente";

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Produto atualizado. Aguarda aprovação." });
        }

        [HttpPut("{id}/suspender")]
        public async Task<IActionResult> SuspenderProduto(int id)
        {
            var fornecedor = await GetFornecedorAtual();
            if (fornecedor == null)
                return Unauthorized(new { Message = "Fornecedor não encontrado." });

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.FornecedorId == fornecedor.Id);

            if (produto == null)
                return NotFound(new { Message = "Produto não encontrado." });

            produto.Estado = "Inativo";
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Produto suspenso." });
        }

        [HttpPut("{id}/reativar")]
        public async Task<IActionResult> ReativarProduto(int id)
        {
            var fornecedor = await GetFornecedorAtual();
            if (fornecedor == null)
                return Unauthorized(new { Message = "Fornecedor não encontrado." });

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.FornecedorId == fornecedor.Id);

            if (produto == null)
                return NotFound(new { Message = "Produto não encontrado." });

            produto.Estado = "Pendente";
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Produto reativado. Aguarda aprovação." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> ApagarProduto(int id)
        {
            var fornecedor = await GetFornecedorAtual();
            if (fornecedor == null)
                return Unauthorized(new { Message = "Fornecedor não encontrado." });

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.FornecedorId == fornecedor.Id);

            if (produto == null)
                return NotFound(new { Message = "Produto não encontrado." });

            var temVendas = await _context.LinhasVenda.AnyAsync(l => l.ProdutoId == id);
            if (temVendas)
                return BadRequest(new { Message = "Existem vendas associadas." });

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Produto apagado." });
        }

        [HttpGet("vendas")]
        public async Task<IActionResult> GetMinhasVendas()
        {
            var fornecedor = await GetFornecedorAtual();
            if (fornecedor == null)
                return Ok(new List<object>()); // Lista vazia

            _logger.LogInformation($"A carregar vendas para FornecedorId={fornecedor.Id}");

            var linhasVenda = await _context.LinhasVenda
                .Include(l => l.Venda)
                    .ThenInclude(v => v!.Cliente)
                        .ThenInclude(c => c!.ApplicationUser)
                .Include(l => l.Produto)
                .Where(l => l.Produto != null && l.Produto.FornecedorId == fornecedor.Id)
                .OrderByDescending(l => l.Venda!.Data)
                .ToListAsync();

            _logger.LogInformation($"Encontradas {linhasVenda.Count} vendas para fornecedor {fornecedor.Id}");

            return Ok(linhasVenda.Select(l => new
            {
                VendaId = l.VendaId,
                Data = l.Venda?.Data,
                Estado = l.Venda?.Estado,
                ProdutoNome = l.Produto?.Nome,
                Quantidade = l.Quantidade,
                PrecoUnitario = l.Produto?.PrecoBase ?? 0,
                Total = (l.Produto?.PrecoBase ?? 0) * l.Quantidade,
                ClienteNome = l.Venda?.Cliente?.ApplicationUser?.NomeCompleto ?? "N/A"
            }));
        }
    }
}
