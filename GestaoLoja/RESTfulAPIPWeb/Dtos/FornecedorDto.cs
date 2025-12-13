namespace RESTfulAPIPWeb.Dtos
{
    public class FornecedorDto
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = default!;
        public string Nome { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string NomeEmpresa { get; set; } = default!;
        public string NIF { get; set; } = default!;
        public string Estado { get; set; } = default!;
    }

    public class FornecedorEstadoUpdateDto
    {
        public string NovoEstado { get; set; } = default!;
    }
}
