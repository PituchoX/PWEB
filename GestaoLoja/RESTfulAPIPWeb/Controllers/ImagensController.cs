using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RESTfulAPIPWeb.Controllers
{
    /// <summary>
    /// Controller para upload de imagens
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ImagensController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ImagensController> _logger;

        public ImagensController(IWebHostEnvironment env, ILogger<ImagensController> logger)
        {
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Obtém o caminho da pasta de imagens (partilhada com GestaoLoja)
        /// </summary>
        private string GetPastaImagens()
        {
            // Tentar usar a pasta da GestaoLoja primeiro
            var caminhoGestaoLoja = Path.Combine(_env.ContentRootPath, "..", "GestaoLoja", "wwwroot", "img");
            
            if (Directory.Exists(Path.GetDirectoryName(caminhoGestaoLoja)))
            {
                if (!Directory.Exists(caminhoGestaoLoja))
                    Directory.CreateDirectory(caminhoGestaoLoja);
                return caminhoGestaoLoja;
            }
            
            // Fallback para a pasta local da API
            var caminhoLocal = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "img");
            if (!Directory.Exists(caminhoLocal))
                Directory.CreateDirectory(caminhoLocal);
            return caminhoLocal;
        }

        /// <summary>
        /// Upload de imagem de produto (Fornecedor)
        /// </summary>
        [HttpPost("produto")]
        [Authorize(Roles = "Fornecedor")]
        public async Task<IActionResult> UploadImagemProduto(IFormFile ficheiro)
        {
            if (ficheiro == null || ficheiro.Length == 0)
                return BadRequest(new { Message = "Nenhum ficheiro enviado." });

            // Validar tamanho (max 5MB)
            if (ficheiro.Length > 5 * 1024 * 1024)
                return BadRequest(new { Message = "A imagem não pode ter mais de 5MB." });

            // Validar tipo
            var tiposPermitidos = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!tiposPermitidos.Contains(ficheiro.ContentType.ToLower()))
                return BadRequest(new { Message = "Tipo de ficheiro não suportado. Use JPEG, PNG, GIF ou WebP." });

            try
            {
                // Gerar nome único
                var extensao = Path.GetExtension(ficheiro.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extensao))
                    extensao = ".png";

                var nomeUnico = $"prod_{Guid.NewGuid():N}{extensao}";

                // Pasta de destino - partilhada com GestaoLoja
                var pastaImg = GetPastaImagens();
                var caminhoCompleto = Path.Combine(pastaImg, nomeUnico);

                // Guardar ficheiro
                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await ficheiro.CopyToAsync(stream);
                }

                _logger.LogInformation($"Imagem guardada: {nomeUnico} em {pastaImg}");

                return Ok(new { 
                    Success = true, 
                    NomeFicheiro = nomeUnico,
                    Message = "Imagem carregada com sucesso."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao guardar imagem");
                return StatusCode(500, new { Message = $"Erro ao guardar imagem: {ex.Message}" });
            }
        }

        /// <summary>
        /// Upload de imagem de categoria (Admin/Funcionário)
        /// </summary>
        [HttpPost("categoria")]
        [Authorize(Roles = "Administrador,Funcionário")]
        public async Task<IActionResult> UploadImagemCategoria(IFormFile ficheiro)
        {
            if (ficheiro == null || ficheiro.Length == 0)
                return BadRequest(new { Message = "Nenhum ficheiro enviado." });

            if (ficheiro.Length > 5 * 1024 * 1024)
                return BadRequest(new { Message = "A imagem não pode ter mais de 5MB." });

            var tiposPermitidos = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!tiposPermitidos.Contains(ficheiro.ContentType.ToLower()))
                return BadRequest(new { Message = "Tipo de ficheiro não suportado." });

            try
            {
                var extensao = Path.GetExtension(ficheiro.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extensao))
                    extensao = ".png";

                var nomeUnico = $"cat_{Guid.NewGuid():N}{extensao}";
                var pastaImg = GetPastaImagens();
                var caminhoCompleto = Path.Combine(pastaImg, nomeUnico);

                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await ficheiro.CopyToAsync(stream);
                }

                return Ok(new { 
                    Success = true, 
                    NomeFicheiro = nomeUnico 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Erro: {ex.Message}" });
            }
        }
    }
}
