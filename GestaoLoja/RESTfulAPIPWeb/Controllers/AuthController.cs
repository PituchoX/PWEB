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

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _context = context;
        }

        /// <summary>
        /// Registo de novo Cliente
        /// Estado inicial: Pendente (necessita aprovação de Admin/Funcionário)
        /// </summary>
        [HttpPost("register-cliente")]
        public async Task<IActionResult> RegisterCliente([FromBody] RegisterClienteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar se email já existe
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { Message = "Este email já está registado." });

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                NomeCompleto = dto.Nome,
                Estado = "Pendente", // Aguarda aprovação
                Perfil = "Cliente"
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

            await _userManager.AddToRoleAsync(user, "Cliente");

            // Criar registo na tabela Clientes
            var cliente = new Cliente
            {
                ApplicationUserId = user.Id,
                NIF = dto.NIF,
                Estado = "Pendente"
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cliente registado com sucesso. Aguarda aprovação." });
        }

        /// <summary>
        /// Registo de novo Fornecedor
        /// Estado inicial: Pendente (necessita aprovação de Admin/Funcionário)
        /// </summary>
        [HttpPost("register-fornecedor")]
        public async Task<IActionResult> RegisterFornecedor([FromBody] RegisterFornecedorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar se email já existe
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { Message = "Este email já está registado." });

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                NomeCompleto = dto.Nome,
                Estado = "Pendente", // Aguarda aprovação
                Perfil = "Fornecedor"
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

            await _userManager.AddToRoleAsync(user, "Fornecedor");

            // Criar registo na tabela Fornecedores
            var fornecedor = new Fornecedor
            {
                ApplicationUserId = user.Id,
                NomeEmpresa = dto.NomeEmpresa,
                NIF = dto.NIF,
                Estado = "Pendente"
            };

            _context.Fornecedores.Add(fornecedor);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Fornecedor registado com sucesso. Aguarda aprovação." });
        }

        /// <summary>
        /// Login - Devolve JWT Token
        /// Apenas utilizadores com estado "Ativo" podem fazer login
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized(new { Message = "Credenciais inválidas." });

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
                return Unauthorized(new { Message = "Credenciais inválidas." });

            // Verificar estado do utilizador
            if (user.Estado != "Ativo")
                return Unauthorized(new { Message = "A sua conta não está ativa. Aguarde aprovação." });

            var roles = await _userManager.GetRolesAsync(user);

            // Gerar JWT Token
            var token = GenerateJwtToken(user, roles);

            return Ok(new
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(4),
                User = new
                {
                    Id = user.Id,
                    Email = user.Email,
                    Nome = user.NomeCompleto,
                    Perfil = user.Perfil,
                    Roles = roles
                }
            });
        }

        /// <summary>
        /// Gera um JWT Token para o utilizador
        /// </summary>
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

            // Adicionar roles como claims
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
