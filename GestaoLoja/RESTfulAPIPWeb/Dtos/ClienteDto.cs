namespace RESTfulAPIPWeb.Dtos
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = default!;
        public string Nome { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string NIF { get; set; } = default!;
        public string Estado { get; set; } = default!;
    }

    public class ClienteEstadoUpdateDto
    {
        public string NovoEstado { get; set; } = default!;
    }
}
