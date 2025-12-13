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
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientesController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ================================================================
        // GET: api/clientes
        // Lista todos os clientes registados
        // ================================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes()
        {
            var clientes = await _context.Clientes
                .Include(c => c.ApplicationUser)
                .ToListAsync();

            var result = clientes.Select(c => new ClienteDto
            {
                Id = c.Id,
                ApplicationUserId = c.ApplicationUserId,
                Nome = c.ApplicationUser?.NomeCompleto ?? "",
                Email = c.ApplicationUser?.Email ?? "",
                NIF = c.NIF,
                Estado = c.ApplicationUser?.Estado ?? c.Estado
            });

            return Ok(result);
        }

        // ================================================================
        // GET: api/clientes/{id}
        // ================================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(string id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == id);

            if (cliente == null)
                return NotFound();

            return Ok(new ClienteDto
            {
                Id = cliente.Id,
                ApplicationUserId = cliente.ApplicationUserId,
                Nome = cliente.ApplicationUser?.NomeCompleto ?? "",
                Email = cliente.ApplicationUser?.Email ?? "",
                NIF = cliente.NIF,
                Estado = cliente.ApplicationUser?.Estado ?? cliente.Estado
            });
        }

        // ================================================================
        // PUT: api/clientes/{id}/estado
        // Altera estado de Pendente → Ativo ou Inativo
        // ================================================================
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> AtualizarEstado(string id, [FromBody] ClienteEstadoUpdateDto dto)
        {
            var validStates = new[] { "Pendente", "Ativo", "Inativo" };

            if (!validStates.Contains(dto.NovoEstado))
                return BadRequest("Estado inválido. Use: Pendente, Ativo ou Inativo.");

            var cliente = await _context.Clientes
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == id);

            if (cliente == null)
                return NotFound();

            cliente.ApplicationUser!.Estado = dto.NovoEstado;
            cliente.Estado = dto.NovoEstado;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Estado atualizado com sucesso." });
        }

        // ================================================================
        // GET: api/clientes/{id}/vendas
        // Lista vendas do cliente
        // ================================================================
        [HttpGet("{id}/vendas")]
        public async Task<IActionResult> GetVendasDoCliente(string id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == id);

            if (cliente == null)
                return NotFound("Cliente não encontrado.");

            var vendas = await _context.Vendas
                .Where(v => v.ClienteId == cliente.Id)
                .Include(v => v.LinhasVenda!)
                    .ThenInclude(l => l.Produto)
                .Select(v => new VendaViewDto
                {
                    Id = v.Id,
                    ClienteNome = cliente.ApplicationUser != null ? cliente.ApplicationUser.NomeCompleto : "",
                    Data = DateTime.Parse(v.Data),
                    Estado = v.Estado,
                    Total = v.LinhasVenda != null ? v.LinhasVenda.Sum(l => l.Preco * l.Quantidade) : 0,
                    Linhas = v.LinhasVenda != null ? v.LinhasVenda.Select(l => new LinhaVendaViewDto
                    {
                        ProdutoNome = l.Produto != null ? l.Produto.Nome : "",
                        Quantidade = l.Quantidade,
                        PrecoUnitario = l.Preco
                    }).ToList() : new List<LinhaVendaViewDto>()
                })
                .ToListAsync();

            return Ok(vendas);
        }
    }
}
