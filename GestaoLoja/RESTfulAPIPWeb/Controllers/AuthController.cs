using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Entities;
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
        /// Registo de novo Cliente - Fica PENDENTE até aprovação
        /// </summary>
        [HttpPost("register-cliente")]
        public async Task<IActionResult> RegisterCliente([FromBody] RegisterClienteRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Dados inválidos.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            // Verificar se email já existe
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { Success = false, Message = "Este email já está registado." });

            // ALTERADO: Estado Pendente - aguarda aprovação
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                NomeCompleto = dto.Nome,
                Estado = "Pendente",  // <-- PENDENTE até admin/funcionário aprovar
                Perfil = "Cliente"
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(new { Success = false, Message = "Erro ao criar utilizador.", Errors = result.Errors.Select(e => e.Description) });

            await _userManager.AddToRoleAsync(user, "Cliente");

            // Gerar NIF aleatório
            string nifFinal = "9" + new Random().Next(10000000, 99999999).ToString();

            // ALTERADO: Cliente também fica Pendente
            var cliente = new Cliente
            {
                ApplicationUserId = user.Id,
                NIF = nifFinal,
                Estado = "Pendente"  // <-- PENDENTE até admin/funcionário aprovar
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

            return Ok(new { Success = true, Message = "Registo efetuado com sucesso! A sua conta aguarda aprovação por um administrador." });
        }

        /// <summary>
        /// Registo de novo Fornecedor - Fica PENDENTE até aprovação
        /// </summary>
        [HttpPost("register-fornecedor")]
        public async Task<IActionResult> RegisterFornecedor([FromBody] RegisterFornecedorRequest dto)
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

            return Ok(new { Success = true, Message = "Fornecedor registado com sucesso! Aguarda aprovação por um administrador." });
        }

        /// <summary>
        /// Login
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest dto)
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
                return Unauthorized(new { Success = false, Message = "Credenciais inválidas." });
            }

            _logger.LogInformation($"User found: {user.Email}, Estado: {user.Estado}, Perfil: {user.Perfil}");

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("Password invalid");
                return Unauthorized(new { Success = false, Message = "Credenciais inválidas." });
            }

            // ALTERADO: Verificar se conta está ativa OU pendente com mensagem específica
            if (user.Estado == "Pendente")
            {
                _logger.LogWarning($"User account pending approval. Estado: {user.Estado}");
                return Unauthorized(new { Success = false, Message = "A sua conta aguarda aprovação por um administrador. Por favor aguarde." });
            }

            if (user.Estado != "Ativo")
            {
                _logger.LogWarning($"User not active. Estado: {user.Estado}");
                return Unauthorized(new { Success = false, Message = $"A sua conta não está ativa. Estado atual: {user.Estado}" });
            }

            // Obter roles do utilizador
            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation($"User roles from DB: {string.Join(", ", roles)}");

            // Se não tem roles atribuídas, atribuir com base no Perfil
            if (!roles.Any() && !string.IsNullOrEmpty(user.Perfil))
            {
                _logger.LogInformation($"No roles found, adding role based on Perfil: {user.Perfil}");
                
                var roleManager = HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
                if (!await roleManager.RoleExistsAsync(user.Perfil))
                {
                    await roleManager.CreateAsync(new IdentityRole(user.Perfil));
                }
                
                await _userManager.AddToRoleAsync(user, user.Perfil);
                roles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation($"Roles after adding: {string.Join(", ", roles)}");
            }

            // Se o utilizador é Cliente, garantir que tem registo na tabela Clientes
            if (user.Perfil == "Cliente")
            {
                var clienteExiste = await _context.Clientes.AnyAsync(c => c.ApplicationUserId == user.Id);
                if (!clienteExiste)
                {
                    _logger.LogInformation("Creating Cliente record automatically...");
                    var cliente = new Cliente
                    {
                        ApplicationUserId = user.Id,
                        NIF = "9" + new Random().Next(10000000, 99999999).ToString(),
                        Estado = "Ativo"
                    };
                    _context.Clientes.Add(cliente);
                    await _context.SaveChangesAsync();
                }
            }

            // Se o utilizador é Fornecedor, garantir que tem registo na tabela Fornecedores
            if (user.Perfil == "Fornecedor")
            {
                var fornecedorExiste = await _context.Fornecedores.AnyAsync(f => f.ApplicationUserId == user.Id);
                if (!fornecedorExiste)
                {
                    _logger.LogInformation("Creating Fornecedor record automatically...");
                    var fornecedor = new Fornecedor
                    {
                        ApplicationUserId = user.Id,
                        NomeEmpresa = user.NomeCompleto + " (Empresa)",
                        NIF = "9" + new Random().Next(10000000, 99999999).ToString(),
                        Estado = "Pendente"
                    };
                    _context.Fornecedores.Add(fornecedor);
                    await _context.SaveChangesAsync();
                }
            }
            
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

    // DTOs simples para autenticação
    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class RegisterClienteRequest
    {
        public string Nome { get; set; } = "";
        public string Email { get; set; } = "";
        public string NIF { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class RegisterFornecedorRequest
    {
        public string Nome { get; set; } = "";
        public string Email { get; set; } = "";
        public string NIF { get; set; } = "";
        public string NomeEmpresa { get; set; } = "";
        public string Password { get; set; } = "";
    }
}