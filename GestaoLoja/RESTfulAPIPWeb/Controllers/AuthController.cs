using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Dtos;
using RESTfulAPIPWeb.Entities;
using RESTfulAPIPWeb.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RESTfulAPIPWeb.Controllers
{
    /// <summary>
    /// Controller para autenticação e registo de utilizadores via JWT
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config,
            AppDbContext context,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Registo de novo Cliente
        /// </summary>
        [HttpPost("register-cliente")]
        public async Task<IActionResult> RegisterCliente([FromBody] RegisterClienteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Dados inválidos.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            // Verificar se email já existe
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { Success = false, Message = "Este email já está registado." });

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                NomeCompleto = dto.Nome,
                Estado = "Ativo",
                Perfil = "Cliente"
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(new { Success = false, Message = "Erro ao criar utilizador.", Errors = result.Errors.Select(e => e.Description) });

            await _userManager.AddToRoleAsync(user, "Cliente");

            // Gerar NIF aleatório
            string nifFinal = "9" + new Random().Next(10000000, 99999999).ToString();

            var cliente = new Cliente
            {
                ApplicationUserId = user.Id,
                NIF = nifFinal,
                Estado = "Ativo"
            };

            _context.Clientes.Add(cliente);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                cliente.NIF = "9" + new Random().Next(10000000, 99999999).ToString();
                await _context.SaveChangesAsync();
            }

            return Ok(new { Success = true, Message = "Registo efetuado com sucesso! Já pode fazer login." });
        }

        /// <summary>
        /// Registo de novo Fornecedor
        /// </summary>
        [HttpPost("register-fornecedor")]
        public async Task<IActionResult> RegisterFornecedor([FromBody] RegisterFornecedorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Dados inválidos.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { Success = false, Message = "Este email já está registado." });

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                NomeCompleto = dto.Nome,
                Estado = "Pendente",
                Perfil = "Fornecedor"
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(new { Success = false, Message = "Erro ao criar utilizador.", Errors = result.Errors.Select(e => e.Description) });

            await _userManager.AddToRoleAsync(user, "Fornecedor");

            // Gerar NIF aleatório
            string nifFinal = "9" + new Random().Next(10000000, 99999999).ToString();

            var fornecedor = new Fornecedor
            {
                ApplicationUserId = user.Id,
                NomeEmpresa = dto.NomeEmpresa,
                NIF = nifFinal,
                Estado = "Pendente"
            };

            _context.Fornecedores.Add(fornecedor);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Fornecedor registado com sucesso. Aguarda aprovação para inserir produtos." });
        }

        /// <summary>
        /// Login
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            _logger.LogInformation("=== LOGIN ATTEMPT ===");
            _logger.LogInformation($"Email: {dto.Email}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid");
                return BadRequest(new { Success = false, Message = "Dados inválidos." });
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                _logger.LogWarning($"User not found with email: {dto.Email}");
                
                // Listar todos os utilizadores para debug
                var allUsers = await _userManager.Users.Select(u => u.Email).ToListAsync();
                _logger.LogInformation($"All users in DB: {string.Join(", ", allUsers)}");
                
                return Unauthorized(new { Success = false, Message = "Credenciais inválidas." });
            }

            _logger.LogInformation($"User found: {user.Email}, Estado: {user.Estado}, Perfil: {user.Perfil}");

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("Password invalid");
                return Unauthorized(new { Success = false, Message = "Credenciais inválidas." });
            }

            if (user.Estado != "Ativo")
            {
                _logger.LogWarning($"User not active. Estado: {user.Estado}");
                return Unauthorized(new { Success = false, Message = $"A sua conta não está ativa. Estado atual: {user.Estado}" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation($"User roles: {string.Join(", ", roles)}");
            
            var token = GenerateJwtToken(user, roles);
            _logger.LogInformation("Login successful, token generated");

            return Ok(new
            {
                Success = true,
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                NomeCompleto = user.NomeCompleto,
                Perfil = user.Perfil,
                Roles = roles
            });
        }

        /// <summary>
        /// Endpoint de teste - lista todos os utilizadores (REMOVER EM PRODUÇÃO)
        /// </summary>
        [HttpGet("debug/users")]
        public async Task<IActionResult> DebugUsers()
        {
            var users = await _userManager.Users
                .Select(u => new { u.Email, u.NomeCompleto, u.Estado, u.Perfil })
                .ToListAsync();
            return Ok(users);
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("nome", user.NomeCompleto),
                new Claim("estado", user.Estado),
                new Claim("perfil", user.Perfil)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "DefaultSecretKeyForDevelopment123!"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}