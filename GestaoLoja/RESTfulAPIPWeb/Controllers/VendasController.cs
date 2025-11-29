using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Dtos;
using RESTfulAPIPWeb.Entities;

namespace RESTfulAPIPWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VendasController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ================================================================
        // POST: api/vendas
        // Criar venda (CLIENTE)
        // ================================================================
        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> CriarVenda([FromBody] List<ItemVendaDto> itens)
        {
            var userId = User.FindFirst("nameidentifier")?.Value;

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cliente == null)
                return Unauthorized("Cliente inválido.");

            if (!itens.Any())
                return BadRequest("Carrinho vazio.");

            // Criar venda
            var venda = new Venda
            {
                ClienteId = cliente.Id,
                Estado = "Pendente",
                Data = DateTime.UtcNow
            };

            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync();

            // Criar linhas da venda
            foreach (var item in itens)
            {
                var produto = await _context.Produtos.FindAsync(item.ProdutoId);

                if (produto == null)
                    return BadRequest($"Produto {item.ProdutoId} não existe.");

                if (produto.Stock < item.Quantidade)
                    return BadRequest($"Stock insuficiente para o produto {produto.Nome}.");

                _context.LinhasVenda.Add(new LinhaVenda
                {
                    VendaId = venda.Id,
                    ProdutoId = produto.Id,
                    Quantidade = item.Quantidade,
                    Preco = produto.PrecoFinal
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Venda criada com sucesso.", venda.Id });
        }

        // ================================================================
        // GET: api/vendas/minhas
        // Histórico do cliente autenticado
        // ================================================================
        [HttpGet("minhas")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MinhasVendas()
        {
            var userId = User.FindFirst("nameidentifier")?.Value;

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cliente == null)
                return Unauthorized();

            var vendas = await _context.Vendas
                .Where(v => v.ClienteId == cliente.Id)
                .Include(v => v.LinhasVenda!)
                .ThenInclude(l => l.Produto)
                .ToListAsync();

            return Ok(vendas);
        }

        // ================================================================
        // GET: api/vendas/fornecedor
        // Vendas dos produtos do fornecedor autenticado
        // ================================================================
        [HttpGet("fornecedor")]
        [Authorize(Roles = "Fornecedor")]
        public async Task<IActionResult> VendasFornecedor()
        {
            var userId = User.FindFirst("nameidentifier")?.Value;

            var fornecedor = await _context.Fornecedores
                .FirstOrDefaultAsync(f => f.ApplicationUserId == userId);

            if (fornecedor == null)
                return Unauthorized();

            var vendas = await _context.LinhasVenda
                .Include(l => l.Venda)
                .Include(l => l.Produto)
                .Where(l => l.Produto!.FornecedorId == fornecedor.Id)
                .ToListAsync();

            return Ok(vendas);
        }

        // ================================================================
        // GET: api/vendas/pendentes
        // Funcionário/Admin: lista vendas pendentes
        // ================================================================
        [HttpGet("pendentes")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> VendasPendentes()
        {
            var vendas = await _context.Vendas
                .Where(v => v.Estado == "Pendente")
                .Include(v => v.Cliente)
                .Include(v => v.LinhasVenda!)
                    .ThenInclude(l => l.Produto)
                .ToListAsync();

            var lista = vendas.Select(v => new VendaViewDto
            {
                Id = v.Id,
                ClienteNome = v.Cliente!.ApplicationUser!.Nome,
                Data = v.Data,
                Estado = v.Estado,
                Total = v.LinhasVenda!.Sum(l => l.Preco * l.Quantidade),
                Linhas = v.LinhasVenda!.Select(l => new LinhaVendaViewDto
                {
                    ProdutoNome = l.Produto!.Nome,
                    Quantidade = l.Quantidade,
                    PrecoUnitario = l.Preco
                }).ToList()
            });

            return Ok(lista);
        }


        // ================================================================
        // PUT: api/vendas/{id}/confirmar
        // Funcionário/Admin confirma venda
        // ================================================================
        [HttpGet("confirmadas")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> VendasConfirmadas()
        {
            var vendas = await _context.Vendas
                .Where(v => v.Estado == "Confirmada")
                .Include(v => v.Cliente)
                .Include(v => v.LinhasVenda!)
                    .ThenInclude(l => l.Produto)
                .ToListAsync();

            var lista = vendas.Select(v => new VendaViewDto
            {
                Id = v.Id,
                ClienteNome = v.Cliente!.ApplicationUser!.Nome,
                Data = v.Data,
                Estado = v.Estado,
                Total = v.LinhasVenda!.Sum(l => l.Preco * l.Quantidade),
                Linhas = v.LinhasVenda!.Select(l => new LinhaVendaViewDto
                {
                    ProdutoNome = l.Produto!.Nome,
                    Quantidade = l.Quantidade,
                    PrecoUnitario = l.Preco
                }).ToList()
            });

            return Ok(lista);
        }


        // ================================================================
        // PUT: api/vendas/{id}/rejeitar
        // Funcionário/Admin rejeita venda
        // ================================================================
        [HttpPut("{id}/rejeitar")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> RejeitarVenda(int id)
        {
            var venda = await _context.Vendas.FindAsync(id);

            if (venda == null)
                return NotFound();

            venda.Estado = "Rejeitada";

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Venda rejeitada." });
        }

        // ================================================================
        // PUT: api/vendas/{id}/expedir
        // Funcionário/Admin expede venda + atualiza stocks
        // ================================================================
        [HttpGet("expedidas")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> VendasExpedidas()
        {
            var vendas = await _context.Vendas
                .Where(v => v.Estado == "Expedida")
                .Include(v => v.Cliente)
                .Include(v => v.LinhasVenda!)
                    .ThenInclude(l => l.Produto)
                .ToListAsync();

            var lista = vendas.Select(v => new VendaViewDto
            {
                Id = v.Id,
                ClienteNome = v.Cliente!.ApplicationUser!.Nome,
                Data = v.Data,
                Estado = v.Estado,
                Total = v.LinhasVenda!.Sum(l => l.Preco * l.Quantidade),
                Linhas = v.LinhasVenda!.Select(l => new LinhaVendaViewDto
                {
                    ProdutoNome = l.Produto!.Nome,
                    Quantidade = l.Quantidade,
                    PrecoUnitario = l.Preco
                }).ToList()
            });

            return Ok(lista);
        }

    }

    // DTO usado ao criar venda (carrinho)
    public class ItemVendaDto
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }
}
