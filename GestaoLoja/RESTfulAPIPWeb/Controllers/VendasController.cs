using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Entities;
using RESTfulAPIPWeb.Dtos;

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
        // CLIENTE CRIA VENDA
        // ================================================================
        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> CriarVenda([FromBody] List<ItemVendaDto> itens)
        {
            if (itens == null || !itens.Any())
                return BadRequest("O carrinho está vazio.");

            var userId = User.FindFirst("nameidentifier")?.Value;

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cliente == null)
                return Unauthorized("Cliente inválido.");

            // Criar nova venda
            var venda = new Venda
            {
                ClienteId = cliente.Id,
                Estado = "Pendente",
                Data = DateTime.UtcNow
            };

            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync();

            foreach (var item in itens)
            {
                var produto = await _context.Produtos.FindAsync(item.ProdutoId);

                if (produto == null)
                    return BadRequest($"Produto com ID {item.ProdutoId} não existe.");

                if (produto.Stock < item.Quantidade)
                    return BadRequest($"Stock insuficiente para o produto '{produto.Nome}'.");

                _context.LinhasVenda.Add(new LinhaVenda
                {
                    VendaId = venda.Id,
                    ProdutoId = produto.Id,
                    Quantidade = item.Quantidade,
                    Preco = produto.PrecoFinal
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Venda criada com sucesso!", venda.Id });
        }

        // ================================================================
        // CLIENTE: HISTÓRICO DE VENDAS
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
                .Include(v => v.LinhasVenda)
                    .ThenInclude(l => l.Produto)
                .ToListAsync();

            return Ok(vendas);
        }

        // ================================================================
        // FORNECEDOR: VENDAS DOS SEUS PRODUTOS
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
        // ADMIN/FUNCIONÁRIO: LISTAR VENDAS POR ESTADO
        // ================================================================
        [Authorize(Roles = "Administrador,Funcionário")]
        [HttpGet("pendentes")]
        public async Task<IActionResult> Pendentes()
        {
            return Ok(await BuscarVendasPorEstado("Pendente"));
        }

        [Authorize(Roles = "Administrador,Funcionário")]
        [HttpGet("confirmadas")]
        public async Task<IActionResult> Confirmadas()
        {
            return Ok(await BuscarVendasPorEstado("Confirmada"));
        }

        [Authorize(Roles = "Administrador,Funcionário")]
        [HttpGet("expedidas")]
        public async Task<IActionResult> Expedidas()
        {
            return Ok(await BuscarVendasPorEstado("Expedida"));
        }

        private async Task<IEnumerable<VendaViewDto>> BuscarVendasPorEstado(string estado)
        {
            var vendas = await _context.Vendas
                .Where(v => v.Estado == estado)
                .Include(v => v.Cliente)
                    .ThenInclude(c => c.ApplicationUser)
                .Include(v => v.LinhasVenda!)
                    .ThenInclude(l => l.Produto)
                .ToListAsync();

            return vendas.Select(v => new VendaViewDto
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
        }

        // ================================================================
        // ADMIN/FUNCIONÁRIO: REJEITAR VENDA
        // ================================================================
        [HttpPut("{id}/rejeitar")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> Rejeitar(int id)
        {
            var venda = await _context.Vendas.FindAsync(id);
            if (venda == null) return NotFound();

            if (venda.Estado == "Expedida")
                return BadRequest("Não é possível rejeitar uma venda já expedida.");

            venda.Estado = "Rejeitada";
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Venda rejeitada com sucesso." });
        }

        // ================================================================
        // ADMIN/FUNCIONÁRIO: CONFIRMAR VENDA
        // ================================================================
        [HttpPut("{id}/confirmar")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> Confirmar(int id)
        {
            var venda = await _context.Vendas
                .Include(v => v.LinhasVenda!)
                    .ThenInclude(l => l.Produto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null)
                return NotFound();

            if (venda.Estado != "Pendente")
                return BadRequest("Apenas vendas pendentes podem ser confirmadas.");

            // Validar stock de todos os produtos
            foreach (var linha in venda.LinhasVenda!)
            {
                if (linha.Produto!.Stock < linha.Quantidade)
                    return BadRequest($"Stock insuficiente para o produto '{linha.Produto.Nome}'.");
            }

            venda.Estado = "Confirmada";
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Venda confirmada." });
        }

        // ================================================================
        // ADMIN/FUNCIONÁRIO: EXPEDIR VENDA
        // ================================================================
        [HttpPut("{id}/expedir")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> Expedir(int id)
        {
            var venda = await _context.Vendas
                .Include(v => v.LinhasVenda!)
                    .ThenInclude(l => l.Produto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null)
                return NotFound();

            if (venda.Estado != "Confirmada")
                return BadRequest("Apenas vendas confirmadas podem ser expedidas.");

            // Atualizar stock
            foreach (var linha in venda.LinhasVenda!)
            {
                linha.Produto!.Stock -= linha.Quantidade;
            }

            venda.Estado = "Expedida";
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Venda expedida com sucesso." });
        }
    }

    // DTOs usados
    public class ItemVendaDto
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }
}
