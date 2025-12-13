using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTfulAPIPWeb.Dtos;
using RESTfulAPIPWeb.Entities;
using RESTfulAPIPWeb.Repositories.Interfaces;

namespace RESTfulAPIPWeb.Controllers
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
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ModoEntregaDto>>> GetModosEntrega()
        {
            var modos = await _repo.GetAllAsync();
            var result = modos.Select(m => new ModoEntregaDto
            {
                Id = m.Id,
                Nome = m.Nome,
                Tipo = m.Tipo,
                Detalhe = m.Detalhe
            });

            return Ok(result);
        }

        // GET: api/modosentrega/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ModoEntregaDto>> GetModoEntrega(int id)
        {
            var modo = await _repo.GetByIdAsync(id);
            if (modo == null) return NotFound();

            return Ok(new ModoEntregaDto
            {
                Id = modo.Id,
                Nome = modo.Nome,
                Tipo = modo.Tipo,
                Detalhe = modo.Detalhe
            });
        }

        // POST: api/modosentrega
        [HttpPost]
        public async Task<ActionResult<ModoEntregaDto>> CreateModoEntrega(ModoEntregaCreateDto dto)
        {
            var modo = new ModoEntrega
            {
                Nome = dto.Nome,
                Tipo = dto.Tipo,
                Detalhe = dto.Detalhe
            };

            var created = await _repo.AddAsync(modo);

            return CreatedAtAction(nameof(GetModoEntrega), new { id = created.Id }, new ModoEntregaDto
            {
                Id = created.Id,
                Nome = created.Nome,
                Tipo = created.Tipo,
                Detalhe = created.Detalhe
            });
        }

        // PUT: api/modosentrega/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateModoEntrega(int id, ModoEntregaDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var modo = new ModoEntrega
            {
                Id = dto.Id,
                Nome = dto.Nome,
                Tipo = dto.Tipo,
                Detalhe = dto.Detalhe
            };

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
