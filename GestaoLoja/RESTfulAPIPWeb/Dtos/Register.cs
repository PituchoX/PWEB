namespace RESTfulAPIPWeb.Dtos
{
    public class RegisterDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";  // "Cliente" ou "Fornecedor"

        // Dados extra conforme o enunciado
        public string Nome { get; set; } = "";
        public string NIF { get; set; } = "";
    }
}
