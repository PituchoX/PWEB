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
    public class FornecedoresController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FornecedoresController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ================================================================
        // GET: api/fornecedores
        // Lista todos os fornecedores
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> GetFornecedores()
        {
            var fornecedores = await _context.Fornecedores
                .Include(f => f.ApplicationUser)
                .ToListAsync();

            return Ok(fornecedores);
        }

        // ================================================================
        // GET: api/fornecedores/{id}
        // ================================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFornecedor(string id)
        {
            var fornecedor = await _context.Fornecedores
                .Include(f => f.ApplicationUser)
                .FirstOrDefaultAsync(f => f.ApplicationUserId == id);

            if (fornecedor == null)
                return NotFound();

            return Ok(fornecedor);
        }

        // ================================================================
        // PUT: api/fornecedores/{id}/estado
        // Alterar estado: Pendente / Ativo / Inativo
        // ================================================================
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> AtualizarEstado(string id, [FromBody] string novoEstado)
        {
            var validStates = new[] { "Pendente", "Ativo", "Inativo" };

            if (!validStates.Contains(novoEstado))
                return BadRequest("Estado inválido. Utilize: Pendente, Ativo ou Inativo.");

            var fornecedor = await _context.Fornecedores
                .Include(f => f.ApplicationUser)
                .FirstOrDefaultAsync(f => f.ApplicationUserId == id);

            if (fornecedor == null)
                return NotFound();

            fornecedor.ApplicationUser!.Estado = novoEstado;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Estado atualizado com sucesso." });
        }

        // ================================================================
        // GET: api/fornecedores/{id}/produtos
        // Lista produtos pertencentes ao fornecedor
        // ================================================================
        [HttpGet("{id}/produtos")]
        public async Task<IActionResult> GetProdutosDoFornecedor(string id)
        {
            var fornecedor = await _context.Fornecedores
                .FirstOrDefaultAsync(f => f.ApplicationUserId == id);

            if (fornecedor == null)
                return NotFound("Fornecedor não encontrado.");

            var produtos = await _context.Produtos
                .Where(p => p.FornecedorId == fornecedor.Id)
                .ToListAsync();

            return Ok(produtos);
        }
    }
}
