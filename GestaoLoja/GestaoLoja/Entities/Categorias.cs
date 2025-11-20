namespace GestaoLoja.Entities
{
    public class Categorias
    {
        public int Id { get; set; }
        public string Nome { get; set; }

        public ICollection<Produtos> Produtos { get; set; }
    }
}
