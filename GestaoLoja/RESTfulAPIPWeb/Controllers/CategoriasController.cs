using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTfulAPIPWeb.Dtos;
using RESTfulAPIPWeb.Repositories.Interfaces;

namespace RESTfulAPIPWeb.Controllers
{
    /// <summary>
    /// Controller para consulta de categorias da API MyMEDIA
    /// Admin/Funcionário gerem categorias na aplicação GestaoLoja
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepository _repo;

        public CategoriasController(ICategoriaRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Lista todas as categorias
        /// </summary>
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

        /// <summary>
        /// Obtém uma categoria pelo ID
        /// </summary>
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
    }
}
