namespace RESTfulAPIPWeb.Dtos
{
    public class ProdutoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = default!;
        public decimal PrecoBase { get; set; }
        public decimal Percentagem { get; set; }
        public decimal PrecoFinal { get; set; }
        public string Estado { get; set; } = "Pendente";
        public int Stock { get; set; }
        public string Imagem { get; set; } = "semfoto.png";
        public int CategoriaId { get; set; }
        public string? CategoriaNome { get; set; }
        public int? SubcategoriaId { get; set; }
        public string? SubcategoriaNome { get; set; }
        public int ModoEntregaId { get; set; }
        public string? ModoEntregaNome { get; set; }
        public int FornecedorId { get; set; }
        public string? FornecedorNome { get; set; }
    }

    public class ProdutoCreateDto
    {
        public string Nome { get; set; } = default!;
        public decimal PrecoBase { get; set; }
        public decimal Percentagem { get; set; }
        public string Estado { get; set; } = "Pendente";
        public int Stock { get; set; }
        public string Imagem { get; set; } = "semfoto.png";
        public int CategoriaId { get; set; }
        public int? SubcategoriaId { get; set; }
        public int ModoEntregaId { get; set; }
        public int FornecedorId { get; set; }
    }

    public class ProdutoUpdateDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = default!;
        public decimal PrecoBase { get; set; }
        public decimal Percentagem { get; set; }
        public string Estado { get; set; } = "Pendente";
        public int Stock { get; set; }
        public string Imagem { get; set; } = "semfoto.png";
        public int CategoriaId { get; set; }
        public int? SubcategoriaId { get; set; }
        public int ModoEntregaId { get; set; }
        public int FornecedorId { get; set; }
    }
}
