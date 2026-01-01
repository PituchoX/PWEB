namespace RESTfulAPIPWeb.Dtos
{
    public class SubcategoriaDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string? Imagem { get; set; }
        public int CategoriaId { get; set; }
        public string? CategoriaNome { get; set; }
    }

    public class SubcategoriaCreateDto
    {
        public string Nome { get; set; } = "";
        public string? Imagem { get; set; }
        public int CategoriaId { get; set; }
    }
}
