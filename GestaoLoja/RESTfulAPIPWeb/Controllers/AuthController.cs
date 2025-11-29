using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Dtos;
using RESTfulAPIPWeb.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RESTfulAPIPWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            AppDbContext context)
        {
            _userManager = userManager;
            _config = config;
            _context = context;
        }

        // ============================
        // POST: api/auth/register
        // ============================
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Nome = dto.Nome,
                NIF = dto.NIF,
                Estado = "Pendente"  // OBRIGATÓRIO segundo o enunciado
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Atribuir role (Cliente ou Fornecedor)
            await _userManager.AddToRoleAsync(user, dto.Role);

            // Criar entidade na tabela certa
            if (dto.Role == "Cliente")
            {
                _context.Clientes.Add(new Cliente
                {
                    ApplicationUserId = user.Id
                });
            }
            else if (dto.Role == "Fornecedor")
            {
                _context.Fornecedores.Add(new Fornecedor
                {
                    ApplicationUserId = user.Id,
                    NomeEmpresa = dto.Nome,
                    NIF = dto.NIF
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Registo criado. Aguarde ativação." });
        }

        // ============================
        // POST: api/auth/login
        // ============================
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Credenciais inválidas");

            if (user.Estado != "Ativo")
                return Unauthorized("A conta ainda não foi ativada.");

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiresInMinutes"]!)),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { Token = tokenString });
        }
    }
}
