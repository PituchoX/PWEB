using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Entities;
using RESTfulAPIPWeb.Dtos;
using System.Security.Claims;

namespace RESTfulAPIPWeb.Controllers
{
    /// <summary>
    /// Controller para gestão de vendas/compras da API MyMEDIA
    /// </summary>
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

        /// <summary>
        /// Cliente cria uma nova venda/encomenda
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> CriarVenda([FromBody] List<ItemVendaDto> itens)
        {
            if (itens == null || !itens.Any())
                return BadRequest(new { Message = "O carrinho está vazio." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var cliente = await _context.Clientes
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cliente == null)
                return Unauthorized(new { Message = "Cliente não encontrado." });

            // Verificar se cliente está ativo
            if (cliente.Estado != "Ativo" && cliente.ApplicationUser?.Estado != "Ativo")
                return BadRequest(new { Message = "A sua conta não está ativa." });

            // Validar todos os produtos antes de criar a venda
            foreach (var item in itens)
            {
                var produto = await _context.Produtos.FindAsync(item.ProdutoId);

                if (produto == null)
                    return BadRequest(new { Message = $"Produto com ID {item.ProdutoId} não existe." });

                if (produto.Estado != "Ativo")
                    return BadRequest(new { Message = $"Produto '{produto.Nome}' não está disponível." });

                if (produto.Stock < item.Quantidade)
                    return BadRequest(new { Message = $"Stock insuficiente para o produto '{produto.Nome}'. Disponível: {produto.Stock}" });
            }

            // Criar nova venda
            var venda = new Venda
            {
                ClienteId = cliente.Id,
                Estado = "Pendente",
                Data = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync();

            decimal total = 0;

            foreach (var item in itens)
            {
                var produto = await _context.Produtos.FindAsync(item.ProdutoId);

                _context.LinhasVenda.Add(new LinhaVenda
                {
                    VendaId = venda.Id,
                    ProdutoId = produto!.Id,
                    Quantidade = item.Quantidade,
                    Preco = produto.PrecoFinal
                });

                total += produto.PrecoFinal * item.Quantidade;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Encomenda criada com sucesso!",
                VendaId = venda.Id,
                Total = total,
                Estado = venda.Estado
            });
        }

        /// <summary>
        /// Cliente consulta o histórico das suas compras
        /// </summary>
        [HttpGet("minhas")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MinhasVendas()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cliente == null)
                return Unauthorized(new { Message = "Cliente não encontrado." });

            var vendas = await _context.Vendas
                .Where(v => v.ClienteId == cliente.Id)
                .Include(v => v.LinhasVenda!)
                    .ThenInclude(l => l.Produto)
                .OrderByDescending(v => v.Data)
                .ToListAsync();

            var result = vendas.Select(v => new
            {
                Id = v.Id,
                Data = v.Data,
                Estado = v.Estado,
                Total = v.LinhasVenda?.Sum(l => l.Preco * l.Quantidade) ?? 0,
                Itens = v.LinhasVenda?.Select(l => new
                {
                    ProdutoId = l.ProdutoId,
                    ProdutoNome = l.Produto?.Nome,
                    Quantidade = l.Quantidade,
                    PrecoUnitario = l.Preco,
                    Subtotal = l.Preco * l.Quantidade
                })
            });

            return Ok(result);
        }

        /// <summary>
        /// Admin/Funcionário lista vendas pendentes
        /// </summary>
        [HttpGet("pendentes")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> VendasPendentes()
        {
            return Ok(await BuscarVendasPorEstado("Pendente"));
        }

        /// <summary>
        /// Admin/Funcionário lista vendas confirmadas
        /// </summary>
        [HttpGet("confirmadas")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> VendasConfirmadas()
        {
            return Ok(await BuscarVendasPorEstado("Confirmada"));
        }

        /// <summary>
        /// Admin/Funcionário lista vendas expedidas
        /// </summary>
        [HttpGet("expedidas")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> VendasExpedidas()
        {
            return Ok(await BuscarVendasPorEstado("Expedida"));
        }

        /// <summary>
        /// Admin/Funcionário lista vendas rejeitadas
        /// </summary>
        [HttpGet("rejeitadas")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> VendasRejeitadas()
        {
            return Ok(await BuscarVendasPorEstado("Rejeitada"));
        }

        /// <summary>
        /// Busca vendas por estado
        /// </summary>
        private async Task<IEnumerable<VendaViewDto>> BuscarVendasPorEstado(string estado)
        {
            var vendas = await _context.Vendas
                .Where(v => v.Estado == estado)
                .Include(v => v.Cliente)
                    .ThenInclude(c => c!.ApplicationUser)
                .Include(v => v.LinhasVenda!)
                    .ThenInclude(l => l.Produto)
                .OrderByDescending(v => v.Data)
                .ToListAsync();

            return vendas.Select(v => new VendaViewDto
            {
                Id = v.Id,
                ClienteNome = v.Cliente?.ApplicationUser?.NomeCompleto ?? "N/A",
                Data = DateTime.TryParse(v.Data, out var dt) ? dt : DateTime.Now,
                Estado = v.Estado,
                Total = v.LinhasVenda?.Sum(l => l.Preco * l.Quantidade) ?? 0,
                Linhas = v.LinhasVenda?.Select(l => new LinhaVendaViewDto
                {
                    ProdutoNome = l.Produto?.Nome ?? "",
                    Quantidade = l.Quantidade,
                    PrecoUnitario = l.Preco
                }).ToList() ?? new List<LinhaVendaViewDto>()
            });
        }

        /// <summary>
        /// Admin/Funcionário confirma uma venda (valida stock)
        /// </summary>
        [HttpPut("{id}/confirmar")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> ConfirmarVenda(int id)
        {
            var venda = await _context.Vendas
                .Include(v => v.LinhasVenda!)
                    .ThenInclude(l => l.Produto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null)
                return NotFound(new { Message = "Venda não encontrada." });

            if (venda.Estado != "Pendente")
                return BadRequest(new { Message = "Apenas vendas pendentes podem ser confirmadas." });

            // Validar stock de todos os produtos
            foreach (var linha in venda.LinhasVenda ?? [])
            {
                if (linha.Produto!.Stock < linha.Quantidade)
                    return BadRequest(new { Message = $"Stock insuficiente para o produto '{linha.Produto.Nome}'." });
            }

            venda.Estado = "Confirmada";
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Venda confirmada com sucesso." });
        }

        /// <summary>
        /// Admin/Funcionário rejeita uma venda
        /// </summary>
        [HttpPut("{id}/rejeitar")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> RejeitarVenda(int id)
        {
            var venda = await _context.Vendas.FindAsync(id);
            if (venda == null)
                return NotFound(new { Message = "Venda não encontrada." });

            if (venda.Estado == "Expedida")
                return BadRequest(new { Message = "Não é possível rejeitar uma venda já expedida." });

            venda.Estado = "Rejeitada";
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Venda rejeitada." });
        }

        /// <summary>
        /// Admin/Funcionário expede uma venda (atualiza stocks)
        /// </summary>
        [HttpPut("{id}/expedir")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> ExpedirVenda(int id)
        {
            var venda = await _context.Vendas
                .Include(v => v.LinhasVenda!)
                    .ThenInclude(l => l.Produto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null)
                return NotFound(new { Message = "Venda não encontrada." });

            if (venda.Estado != "Confirmada")
                return BadRequest(new { Message = "Apenas vendas confirmadas podem ser expedidas." });

            // Atualizar stock
            foreach (var linha in venda.LinhasVenda ?? [])
            {
                if (linha.Produto!.Stock < linha.Quantidade)
                    return BadRequest(new { Message = $"Stock insuficiente para '{linha.Produto.Nome}'." });

                linha.Produto.Stock -= linha.Quantidade;
            }

            venda.Estado = "Expedida";
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Venda expedida com sucesso. Stocks atualizados." });
        }

        /// <summary>
        /// Simula pagamento de uma venda
        /// </summary>
        [HttpPost("{id}/pagar")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> SimularPagamento(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cliente == null)
                return Unauthorized(new { Message = "Cliente não encontrado." });

            var venda = await _context.Vendas
                .Include(v => v.LinhasVenda)
                .FirstOrDefaultAsync(v => v.Id == id && v.ClienteId == cliente.Id);

            if (venda == null)
                return NotFound(new { Message = "Venda não encontrada." });

            var total = venda.LinhasVenda?.Sum(l => l.Preco * l.Quantidade) ?? 0;

            // Simulação de pagamento
            return Ok(new
            {
                Message = "Pagamento simulado com sucesso!",
                VendaId = venda.Id,
                Total = total,
                MetodoPagamento = "Simulado",
                DataPagamento = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// DTO para item de venda
    /// </summary>
    public class ItemVendaDto
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }
}
