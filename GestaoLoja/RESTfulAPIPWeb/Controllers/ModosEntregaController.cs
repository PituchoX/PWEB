using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTfulAPIPWeb.Dtos;
using RESTfulAPIPWeb.Repositories.Interfaces;

namespace RESTfulAPIPWeb.Controllers
{
    /// <summary>
    /// Controller para consulta de modos de entrega da API MyMEDIA
    /// Admin/Funcionário gerem modos de entrega na aplicação GestaoLoja
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ModosEntregaController : ControllerBase
    {
        private readonly IModoEntregaRepository _repo;

        public ModosEntregaController(IModoEntregaRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Lista todos os modos de entrega
        /// </summary>
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

        /// <summary>
        /// Obtém um modo de entrega pelo ID
        /// </summary>
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
    }
}
