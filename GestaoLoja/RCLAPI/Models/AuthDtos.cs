namespace RCLAPI.Models
{
    public class LoginDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class RegisterClienteDto
    {
        public string NomeCompleto { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string NIF { get; set; } = "";
    }

    public class RegisterFornecedorDto
    {
        public string NomeCompleto { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string NIF { get; set; } = "";
        public string NomeEmpresa { get; set; } = "";
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
