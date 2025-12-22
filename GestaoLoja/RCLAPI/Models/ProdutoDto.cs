namespace RCLAPI.Models
{
    public class ProdutoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public decimal PrecoBase { get; set; }
        public decimal Percentagem { get; set; }
        public decimal PrecoFinal { get; set; }
        public string Estado { get; set; } = "Ativo";
        public int Stock { get; set; }
        public string Imagem { get; set; } = "semfoto.png";
        public int CategoriaId { get; set; }
        public string? CategoriaNome { get; set; }
        public int ModoEntregaId { get; set; }
        public string? ModoEntregaNome { get; set; }
        public int FornecedorId { get; set; }
        public string? FornecedorNome { get; set; }
    }

    public class ProdutoCreateDto
    {
        public string Nome { get; set; } = "";
        public decimal PrecoBase { get; set; }
        public int Stock { get; set; }
        public string Imagem { get; set; } = "semfoto.png";
        public int CategoriaId { get; set; }
        public int ModoEntregaId { get; set; }
    }

    public class ProdutoUpdateDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public decimal PrecoBase { get; set; }
        public int Stock { get; set; }
        public string Imagem { get; set; } = "semfoto.png";
        public int CategoriaId { get; set; }
        public int ModoEntregaId { get; set; }
    }
}
