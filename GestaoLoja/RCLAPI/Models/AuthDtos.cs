using System.ComponentModel.DataAnnotations;

namespace RCLAPI.Models
{
    public class LoginDto
    {
        [Required(ErrorMessage = "O Email é obrigatório")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "A Password é obrigatória")]
        public string Password { get; set; } = "";
    }

    public class RegisterClienteDto
    {
        [Required(ErrorMessage = "O Nome é obrigatório")]
        public string NomeCompleto { get; set; } = "";

        [Required(ErrorMessage = "O Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "A Password é obrigatória")]
        [MinLength(6, ErrorMessage = "A password deve ter pelo menos 6 caracteres")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "A Confirmação é obrigatória")]
        [Compare("Password", ErrorMessage = "As passwords não coincidem")]
        public string ConfirmPassword { get; set; } = "";

        // NIF deixa de ser obrigatório no preenchimento
        public string? NIF { get; set; }
    }

    public class RegisterFornecedorDto
    {
        [Required(ErrorMessage = "O Nome é obrigatório")]
        public string NomeCompleto { get; set; } = "";

        [Required(ErrorMessage = "O Nome da Empresa é obrigatório")]
        public string NomeEmpresa { get; set; } = "";

        [Required(ErrorMessage = "O Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "A Password é obrigatória")]
        [MinLength(6, ErrorMessage = "A password deve ter pelo menos 6 caracteres")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "A Confirmação é obrigatória")]
        [Compare("Password", ErrorMessage = "As passwords não coincidem")]
        public string ConfirmPassword { get; set; } = "";

        public string? NIF { get; set; }
    }

    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? NomeCompleto { get; set; }
        public string? Perfil { get; set; }
        public string? Message { get; set; }
    }

    public class UserInfoDto
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public string NomeCompleto { get; set; } = "";
        public string Perfil { get; set; } = "";
    }
}