namespace RESTfulAPIPWeb.Dtos
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = default!;
        public string? Imagem { get; set; }
    }

    public class CategoriaCreateDto
    {
        public string Nome { get; set; } = default!;
        public string? Imagem { get; set; }
    }
}
