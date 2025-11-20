namespace GestaoLoja.Entities
{
    public class Produtos
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
        public string Imagem { get; set; }

        public int CategoriaId { get; set; }
        public Categorias Categoria { get; set; }

        public int ModoEntregaId { get; set; }
        public ModoEntrega ModoEntrega { get; set; }
    }
}
