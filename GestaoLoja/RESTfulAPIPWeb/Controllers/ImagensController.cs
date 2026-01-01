using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RESTfulAPIPWeb.Controllers
{
    /// <summary>
    /// Controller para upload de imagens
    /// As imagens são guardadas na pasta da GestaoLoja para serem partilhadas
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ImagensController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ImagensController> _logger;
        private readonly IConfiguration _config;

        public ImagensController(IWebHostEnvironment env, ILogger<ImagensController> logger, IConfiguration config)
        {
            _env = env;
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// Obtém o caminho da pasta de imagens (partilhada com GestaoLoja)
        /// </summary>
        private string GetPastaImagens()
        {
            // Prioridade 1: Configuração no appsettings.json
            var caminhoConfig = _config["ImagensPath"];
            if (!string.IsNullOrEmpty(caminhoConfig) && Directory.Exists(Path.GetDirectoryName(caminhoConfig)))
            {
                if (!Directory.Exists(caminhoConfig))
                    Directory.CreateDirectory(caminhoConfig);
                _logger.LogInformation($"Usando caminho de imagens configurado: {caminhoConfig}");
                return caminhoConfig;
            }

            // Prioridade 2: Pasta da GestaoLoja (caminho relativo ao projeto)
            // A API está em: .../GestaoLoja/RESTfulAPIPWeb/
            // A GestaoLoja está em: .../GestaoLoja/GestaoLoja/
            var baseDir = _env.ContentRootPath; // .../RESTfulAPIPWeb
            var parentDir = Directory.GetParent(baseDir)?.FullName; // .../GestaoLoja (pasta da solução)
            
            if (parentDir != null)
            {
                var caminhoGestaoLoja = Path.Combine(parentDir, "GestaoLoja", "wwwroot", "img");
                _logger.LogInformation($"Tentando caminho GestaoLoja: {caminhoGestaoLoja}");
                
                // Verificar se a pasta GestaoLoja existe
                var pastaGestaoLoja = Path.Combine(parentDir, "GestaoLoja", "wwwroot");
                if (Directory.Exists(pastaGestaoLoja))
                {
                    if (!Directory.Exists(caminhoGestaoLoja))
                    {
                        Directory.CreateDirectory(caminhoGestaoLoja);
                        _logger.LogInformation($"Pasta criada: {caminhoGestaoLoja}");
                    }
                    _logger.LogInformation($"Usando pasta GestaoLoja: {caminhoGestaoLoja}");
                    return caminhoGestaoLoja;
                }
            }
            
            // Prioridade 3: Fallback para a pasta local da API
            var caminhoLocal = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "img");
            if (!Directory.Exists(caminhoLocal))
                Directory.CreateDirectory(caminhoLocal);
            
            _logger.LogWarning($"Usando fallback local (imagens não serão visíveis na GestaoLoja): {caminhoLocal}");
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
                return BadRequest(new { Success = false, Message = "Nenhum ficheiro enviado." });

            // Validar tamanho (max 5MB)
            if (ficheiro.Length > 5 * 1024 * 1024)
                return BadRequest(new { Success = false, Message = "A imagem não pode ter mais de 5MB." });

            // Validar tipo
            var tiposPermitidos = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!tiposPermitidos.Contains(ficheiro.ContentType.ToLower()))
                return BadRequest(new { Success = false, Message = "Tipo de ficheiro não suportado. Use JPEG, PNG, GIF ou WebP." });

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

                _logger.LogInformation($"Imagem de produto guardada: {nomeUnico} em {pastaImg}");

                return Ok(new { 
                    Success = true, 
                    NomeFicheiro = nomeUnico,
                    Message = "Imagem carregada com sucesso."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao guardar imagem de produto");
                return StatusCode(500, new { Success = false, Message = $"Erro ao guardar imagem: {ex.Message}" });
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
                return BadRequest(new { Success = false, Message = "Nenhum ficheiro enviado." });

            if (ficheiro.Length > 5 * 1024 * 1024)
                return BadRequest(new { Success = false, Message = "A imagem não pode ter mais de 5MB." });

            var tiposPermitidos = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!tiposPermitidos.Contains(ficheiro.ContentType.ToLower()))
                return BadRequest(new { Success = false, Message = "Tipo de ficheiro não suportado." });

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

                _logger.LogInformation($"Imagem de categoria guardada: {nomeUnico} em {pastaImg}");

                return Ok(new { 
                    Success = true, 
                    NomeFicheiro = nomeUnico 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao guardar imagem de categoria");
                return StatusCode(500, new { Success = false, Message = $"Erro: {ex.Message}" });
            }
        }

        /// <summary>
        /// Endpoint de diagnóstico para verificar caminhos (apenas em desenvolvimento)
        /// </summary>
        [HttpGet("debug/paths")]
        [AllowAnonymous]
        public IActionResult GetPaths()
        {
            if (!_env.IsDevelopment())
                return NotFound();

            var pastaImagens = GetPastaImagens();
            
            return Ok(new
            {
                ContentRootPath = _env.ContentRootPath,
                WebRootPath = _env.WebRootPath,
                PastaImagensUsada = pastaImagens,
                PastaExiste = Directory.Exists(pastaImagens),
                FicheirosNaPasta = Directory.Exists(pastaImagens) 
                    ? Directory.GetFiles(pastaImagens).Select(Path.GetFileName).ToList() 
                    : new List<string?>()
            });
        }
    }
}
