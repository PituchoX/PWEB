using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Dtos;
using RESTfulAPIPWeb.Entities;
using System.Security.Claims;

namespace RESTfulAPIPWeb.Controllers
{
    /// <summary>
    /// Controller para Fornecedores gerirem os seus próprios produtos
    /// Conforme enunciado: Fornecedores podem inserir, consultar, editar produtos seus
    /// Produtos inseridos/editados ficam no estado Pendente até aprovação
    /// </summary>
    [Route("api/fornecedor/produtos")]
    [ApiController]
    [Authorize(Roles = "Fornecedor")]
    public class FornecedorProdutosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FornecedorProdutosController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém o ID do fornecedor do utilizador atual
        /// </summary>
        private async Task<Fornecedor?> GetFornecedorAtual()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return null;

            return await _context.Fornecedores
                .FirstOrDefaultAsync(f => f.ApplicationUserId == userId);
        }

        /// <summary>
        /// Lista os produtos do fornecedor atual
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetMeusProdutos()
        {
            var fornecedor = await GetFornecedorAtual();
            if (fornecedor == null)
                return Unauthorized(new { Message = "Fornecedor não encontrado." });

            var produtos = await _context.Produtos
                .Where(p => p.FornecedorId == fornecedor.Id)
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .OrderByDescending(p => p.Id)
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
                FornecedorNome = fornecedor.NomeEmpresa
            });

            return Ok(result);
        }

        /// <summary>
        /// Obtém um produto do fornecedor pelo ID
        /// </summary>
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
                return NotFound(new { Message = "Produto não encontrado ou não pertence a este fornecedor." });

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

        /// <summary>
        /// Cria um novo produto (estado inicial: Pendente)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProdutoDto>> CriarProduto([FromBody] ProdutoCreateDto dto)
        {
            var fornecedor = await GetFornecedorAtual();
            if (fornecedor == null)
                return Unauthorized(new { Message = "Fornecedor não encontrado." });

            // Verificar se fornecedor está aprovado
            if (fornecedor.Estado != "Aprovado")
                return BadRequest(new { Message = "O seu registo de fornecedor ainda não foi aprovado." });

            var produto = new Produto
            {
                Nome = dto.Nome,
                PrecoBase = dto.PrecoBase,
                Percentagem = 0, // Percentagem definida pelo Admin/Funcionário
                PrecoFinal = dto.PrecoBase, // Será recalculado na aprovação
                Estado = "Pendente", // Sempre pendente até aprovação
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

        /// <summary>
        /// Atualiza um produto existente (volta ao estado Pendente)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarProduto(int id, [FromBody] ProdutoUpdateDto dto)
        {
            var fornecedor = await GetFornecedorAtual();
            if (fornecedor == null)
                return Unauthorized(new { Message = "Fornecedor não encontrado." });

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.FornecedorId == fornecedor.Id);

            if (produto == null)
                return NotFound(new { Message = "Produto não encontrado ou não pertence a este fornecedor." });

            // Atualizar campos permitidos
            produto.Nome = dto.Nome;
            produto.PrecoBase = dto.PrecoBase;
            produto.Stock = dto.Stock;
            produto.Imagem = string.IsNullOrEmpty(dto.Imagem) ? produto.Imagem : dto.Imagem;
            produto.CategoriaId = dto.CategoriaId;
            produto.ModoEntregaId = dto.ModoEntregaId;
            
            // Após edição, volta para Pendente (necessita nova aprovação)
            produto.Estado = "Pendente";

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Produto atualizado. Aguarda nova aprovação." });
        }

        /// <summary>
        /// Suspende um produto (retira da listagem/venda)
        /// </summary>
        [HttpPut("{id}/suspender")]
        public async Task<IActionResult> SuspenderProduto(int id)
        {
            var fornecedor = await GetFornecedorAtual();
            if (fornecedor == null)
                return Unauthorized(new { Message = "Fornecedor não encontrado." });

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.FornecedorId == fornecedor.Id);

            if (produto == null)
                return NotFound(new { Message = "Produto não encontrado ou não pertence a este fornecedor." });

            produto.Estado = "Inativo";
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Produto suspenso com sucesso." });
        }

        /// <summary>
        /// Reativa um produto suspenso (volta para Pendente para nova aprovação)
        /// </summary>
        [HttpPut("{id}/reativar")]
        public async Task<IActionResult> ReativarProduto(int id)
        {
            var fornecedor = await GetFornecedorAtual();
            if (fornecedor == null)
                return Unauthorized(new { Message = "Fornecedor não encontrado." });

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.FornecedorId == fornecedor.Id);

            if (produto == null)
                return NotFound(new { Message = "Produto não encontrado ou não pertence a este fornecedor." });

            if (produto.Estado == "Ativo")
                return BadRequest(new { Message = "Produto já está ativo." });

            produto.Estado = "Pendente";
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Produto reativado. Aguarda aprovação." });
        }

        /// <summary>
        /// Apaga um produto (só se não tiver vendas associadas)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> ApagarProduto(int id)
        {
            var fornecedor = await GetFornecedorAtual();
            if (fornecedor == null)
                return Unauthorized(new { Message = "Fornecedor não encontrado." });

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.FornecedorId == fornecedor.Id);

            if (produto == null)
                return NotFound(new { Message = "Produto não encontrado ou não pertence a este fornecedor." });

            // Verificar se existem vendas associadas
            var temVendas = await _context.LinhasVenda.AnyAsync(l => l.ProdutoId == id);
            if (temVendas)
                return BadRequest(new { Message = "Não é possível apagar este produto porque existem vendas associadas." });

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Produto apagado com sucesso." });
        }

        /// <summary>
        /// Consulta o histórico de vendas dos produtos deste fornecedor
        /// Mostra o PrecoBase (valor que o fornecedor recebe), não o PrecoFinal
        /// </summary>
        [HttpGet("vendas")]
        public async Task<IActionResult> GetMinhasVendas()
        {
            var fornecedor = await GetFornecedorAtual();
            if (fornecedor == null)
                return Unauthorized(new { Message = "Fornecedor não encontrado." });

            var linhasVenda = await _context.LinhasVenda
                .Include(l => l.Venda)
                    .ThenInclude(v => v!.Cliente)
                        .ThenInclude(c => c!.ApplicationUser)
                .Include(l => l.Produto)
                .Where(l => l.Produto != null && l.Produto.FornecedorId == fornecedor.Id)
                .OrderByDescending(l => l.Venda!.Data)
                .ToListAsync();

            // Para o fornecedor, mostramos o PrecoBase (o que ele recebe)
            // não o PrecoFinal (que inclui a percentagem da empresa)
            var result = linhasVenda.Select(l => new
            {
                VendaId = l.VendaId,
                Data = l.Venda?.Data,
                Estado = l.Venda?.Estado,
                ProdutoNome = l.Produto?.Nome,
                Quantidade = l.Quantidade,
                PrecoUnitario = l.Produto?.PrecoBase ?? 0, // Preço Base (valor do fornecedor)
                Total = (l.Produto?.PrecoBase ?? 0) * l.Quantidade, // Total com Preço Base
                ClienteNome = l.Venda?.Cliente?.ApplicationUser?.NomeCompleto ?? "N/A"
            });

            return Ok(result);
        }
    }
}
