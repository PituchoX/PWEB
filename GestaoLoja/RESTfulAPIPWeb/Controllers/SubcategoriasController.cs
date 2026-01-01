using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Dtos;
using RESTfulAPIPWeb.Entities;

namespace RESTfulAPIPWeb.Controllers
{
    /// <summary>
    /// Controller para gestão de subcategorias da API MyMEDIA
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SubcategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SubcategoriasController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista todas as subcategorias
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SubcategoriaDto>>> GetSubcategorias()
        {
            var subcategorias = await _context.Subcategorias
                .Include(s => s.Categoria)
                .OrderBy(s => s.Categoria!.Nome)
                .ThenBy(s => s.Nome)
                .ToListAsync();

            var result = subcategorias.Select(s => new SubcategoriaDto
            {
                Id = s.Id,
                Nome = s.Nome,
                Imagem = s.Imagem,
                CategoriaId = s.CategoriaId,
                CategoriaNome = s.Categoria?.Nome
            });

            return Ok(result);
        }

        /// <summary>
        /// Lista subcategorias de uma categoria específica
        /// </summary>
        [HttpGet("categoria/{categoriaId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SubcategoriaDto>>> GetSubcategoriasPorCategoria(int categoriaId)
        {
            var subcategorias = await _context.Subcategorias
                .Include(s => s.Categoria)
                .Where(s => s.CategoriaId == categoriaId)
                .OrderBy(s => s.Nome)
                .ToListAsync();

            var result = subcategorias.Select(s => new SubcategoriaDto
            {
                Id = s.Id,
                Nome = s.Nome,
                Imagem = s.Imagem,
                CategoriaId = s.CategoriaId,
                CategoriaNome = s.Categoria?.Nome
            });

            return Ok(result);
        }

        /// <summary>
        /// Obtém uma subcategoria pelo ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<SubcategoriaDto>> GetSubcategoria(int id)
        {
            var subcategoria = await _context.Subcategorias
                .Include(s => s.Categoria)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subcategoria == null) 
                return NotFound();

            return Ok(new SubcategoriaDto
            {
                Id = subcategoria.Id,
                Nome = subcategoria.Nome,
                Imagem = subcategoria.Imagem,
                CategoriaId = subcategoria.CategoriaId,
                CategoriaNome = subcategoria.Categoria?.Nome
            });
        }
    }
}
