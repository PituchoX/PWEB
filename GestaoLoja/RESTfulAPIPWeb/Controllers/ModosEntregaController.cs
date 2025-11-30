using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTfulAPIPWeb.Entities;
using RESTfulAPIPWeb.Repositories.Interfaces;
using RESTfulAPIPWeb.Repositories.Interfaces;

namespace RESTfulAPIWeb.Controllers
{
    [Authorize(Roles = "Administrador,Funcionário")]
    [Route("api/[controller]")]
    [ApiController]
    public class ModosEntregaController : ControllerBase
    {

        private readonly IModoEntregaRepository _repo;

        public ModosEntregaController(IModoEntregaRepository repo)
        {
            _repo = repo;
        }

        // GET: api/modosentrega
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModoEntrega>>> GetModosEntrega()
        {
            return Ok(await _repo.GetAllAsync());
        }

        // GET: api/modosentrega/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ModoEntrega>> GetModoEntrega(int id)
        {
            var modo = await _repo.GetByIdAsync(id);
            if (modo == null) return NotFound();

            return Ok(modo);
        }

        // POST: api/modosentrega
        [HttpPost]
        public async Task<ActionResult<ModoEntrega>> CreateModoEntrega(ModoEntrega modo)
        {
            await _repo.AddAsync(modo);
            return CreatedAtAction(nameof(GetModoEntrega), new { id = modo.Id }, modo);
        }

        // PUT: api/modosentrega/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateModoEntrega(int id, ModoEntrega modo)
        {
            if (id != modo.Id) return BadRequest();

            var ok = await _repo.UpdateAsync(modo);
            if (!ok) return NotFound();

            return NoContent();
        }

        // DELETE: api/modosentrega/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModoEntrega(int id)
        {
            var ok = await _repo.DeleteAsync(id);
            if (!ok) return NotFound();

            return NoContent();
        }
    }
}
