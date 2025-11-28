namespace RESTfulAPI.Entities
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = default!;
        public decimal PrecoBase { get; set; }
        public decimal Percentagem { get; set; }
        public decimal PrecoFinal { get; set; }

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        public int ModoEntregaId { get; set; }
        public ModoEntrega? ModoEntrega { get; set; }
    }
}
