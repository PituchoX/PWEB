namespace GestaoLoja.Entities
{
    public class Categorias
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Imagem { get; set; }

        // Subcategorias desta categoria
        public ICollection<Subcategoria>? Subcategorias { get; set; }

        // Produtos desta categoria
        public ICollection<Produtos>? Produtos { get; set; }
    }
}
