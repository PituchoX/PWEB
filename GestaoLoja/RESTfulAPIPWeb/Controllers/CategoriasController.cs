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
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepository _repo;

        public CategoriasController(ICategoriaRepository repo)
        {
            _repo = repo;
        }

        // GET: api/categorias
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetCategorias()
        {
            var categorias = await _repo.GetAllAsync();
            var result = categorias.Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Imagem = c.Imagem
            });

            return Ok(result);
        }

        // GET: api/categorias/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoriaDto>> GetCategoria(int id)
        {
            var categoria = await _repo.GetByIdAsync(id);
            if (categoria == null) return NotFound();

            return Ok(new CategoriaDto
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                Imagem = categoria.Imagem
            });
        }

        // POST: api/categorias
        [HttpPost]
        public async Task<ActionResult<CategoriaDto>> CreateCategoria(CategoriaCreateDto dto)
        {
            var categoria = new Categoria
            {
                Nome = dto.Nome,
                Imagem = dto.Imagem
            };

            var created = await _repo.AddAsync(categoria);

            return CreatedAtAction(nameof(GetCategoria), new { id = created.Id }, new CategoriaDto
            {
                Id = created.Id,
                Nome = created.Nome,
                Imagem = created.Imagem
            });
        }

        // PUT: api/categorias/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategoria(int id, CategoriaDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var categoria = new Categoria
            {
                Id = dto.Id,
                Nome = dto.Nome,
                Imagem = dto.Imagem
            };

            var ok = await _repo.UpdateAsync(categoria);
            if (!ok) return NotFound();

            return NoContent();
        }

        // DELETE: api/categorias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var ok = await _repo.DeleteAsync(id);
            if (!ok) return NotFound();

            return NoContent();
        }
    }
}
