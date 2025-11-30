using Microsoft.AspNetCore.Authorization;
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

        // ===========================================
        // REGISTER CLIENTE
        // ===========================================
        [HttpPost("register-cliente")]
        public async Task<IActionResult> RegisterCliente(RegisterClienteDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Nome = dto.Nome,
                NIF = dto.NIF,
                Estado = "Pendente"
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Cliente");

            _context.Clientes.Add(new Cliente
            {
                ApplicationUserId = user.Id
            });

            await _context.SaveChangesAsync();

            return Ok("Cliente registado com sucesso.");
        }

        // ===========================================
        // REGISTER FORNECEDOR
        // ===========================================
        [HttpPost("register-fornecedor")]
        public async Task<IActionResult> RegisterFornecedor(RegisterFornecedorDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Nome = dto.Nome,
                NIF = dto.NIF,
                Estado = "Pendente"
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Fornecedor");

            _context.Fornecedores.Add(new Fornecedor
            {
                ApplicationUserId = user.Id,
                NomeEmpresa = dto.NomeEmpresa,
                NIF = dto.NIF
            });

            await _context.SaveChangesAsync();

            return Ok("Fornecedor registado com sucesso.");
        }

        // ===========================================
        // LOGIN (DEVOLVE JWT)
        // ===========================================
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized("Credenciais inválidas.");

            var passwordOK = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!passwordOK)
                return Unauthorized("Credenciais inválidas.");

            if (user.Estado != "Ativo")
                return Unauthorized("A sua conta não está ativa.");

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("estado", user.Estado)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }
    }
}
