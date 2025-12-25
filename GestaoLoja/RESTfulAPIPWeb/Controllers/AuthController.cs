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
                Estado = "Ativo",  // <<< ALTERADO: Conta ativa automaticamente
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
                Estado = "Ativo"  // <<< ALTERADO: Registo ativo automaticamente
            };

            _context.Clientes.Add(cliente);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Se der azar e gerar um NIF repetido (raro), tentamos mais uma vez
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
                Estado = "Pendente",  // Fornecedor mantém pendente - precisa aprovação para inserir produtos
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
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Dados inválidos." });

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized(new { Success = false, Message = "Credenciais inválidas." });

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
                return Unauthorized(new { Success = false, Message = "Credenciais inválidas." });

            if (user.Estado != "Ativo")
                return Unauthorized(new { Success = false, Message = "A sua conta não está ativa. Aguarde aprovação." });

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            return Ok(new
            {
                Success = true,  // <<< ADICIONADO: Para o frontend saber que funcionou
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
}