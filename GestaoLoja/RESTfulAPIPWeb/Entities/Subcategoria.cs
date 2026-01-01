namespace RESTfulAPIPWeb.Entities
{
    /// <summary>
    /// Subcategoria permite hierarquia dentro de uma categoria
    /// Exemplo: Categoria "Filmes" -> Subcategorias "Ação", "Comédia", "Drama"
    /// </summary>
    public class Subcategoria
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Imagem { get; set; }

        // Ligação à categoria pai
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        // Produtos desta subcategoria
        public ICollection<Produto>? Produtos { get; set; }
    }
}
