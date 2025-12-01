namespace GestaoLoja.Entities
{
    public class ModoEntrega
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Detalhe { get; set; } = string.Empty;

        public ICollection<Produtos>? Produtos { get; set; }
    }
}
