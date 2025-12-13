namespace RESTfulAPIPWeb.Dtos
{
    public class ModoEntregaDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = default!;
        public string Tipo { get; set; } = default!;
        public string Detalhe { get; set; } = default!;
    }

    public class ModoEntregaCreateDto
    {
        public string Nome { get; set; } = default!;
        public string Tipo { get; set; } = default!;
        public string Detalhe { get; set; } = default!;
    }
}
