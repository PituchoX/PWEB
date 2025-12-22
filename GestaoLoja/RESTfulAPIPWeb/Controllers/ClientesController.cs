using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Dtos;
using RESTfulAPIPWeb.Entities;

namespace RESTfulAPIPWeb.Controllers
{
    /// <summary>
    /// Controller para consulta de clientes da API MyMEDIA
    /// Admin/Funcionário gerem clientes na aplicação GestaoLoja
    /// </summary>
    [Authorize(Roles = "Administrador,Funcionário")]
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista todos os clientes registados
        /// </summary>
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

        /// <summary>
        /// Obtém um cliente pelo ID
        /// </summary>
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

        /// <summary>
        /// Lista vendas de um cliente específico
        /// </summary>
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
