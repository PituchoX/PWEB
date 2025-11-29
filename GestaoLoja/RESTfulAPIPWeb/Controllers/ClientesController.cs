using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Entities;

namespace RESTfulAPIPWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador,Funcionário")]
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
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _context.Clientes
                .Include(c => c.ApplicationUser)
                .ToListAsync();

            return Ok(clientes);
        }

        // ================================================================
        // GET: api/clientes/{id}
        // ================================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCliente(string id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == id);

            if (cliente == null)
                return NotFound();

            return Ok(cliente);
        }

        // ================================================================
        // PUT: api/clientes/{id}/estado
        // Altera estado de Pendente → Ativo ou Inativo
        // ================================================================
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> AtualizarEstado(string id, [FromBody] string novoEstado)
        {
            var validStates = new[] { "Pendente", "Ativo", "Inativo" };

            if (!validStates.Contains(novoEstado))
                return BadRequest("Estado inválido. Use: Pendente, Ativo ou Inativo.");

            var cliente = await _context.Clientes
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == id);

            if (cliente == null)
                return NotFound();

            cliente.ApplicationUser!.Estado = novoEstado;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Estado atualizado com sucesso." });
        }
    }
}
