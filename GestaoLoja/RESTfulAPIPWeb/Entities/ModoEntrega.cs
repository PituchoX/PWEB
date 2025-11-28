namespace RESTfulAPI.Entities
{
    public class ModoEntrega
    {
        public int Id { get; set; }
        public string Nome { get; set; } = default!;
        public string Tipo { get; set; } = default!;
        public string Detalhe { get; set; } = default!;
    }
}
