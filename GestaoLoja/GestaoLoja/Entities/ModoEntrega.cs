namespace GestaoLoja.Entities
{
    public class ModoEntrega
    {
        public int Id { get; set; }
        public string Tipo { get; set; }

        public ICollection<Produtos> Produtos { get; set; }
    }
}
