namespace RCLAPI.Models
{
    public class ItemCarrinhoDto
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = "";
        public string ProdutoImagem { get; set; } = "semfoto.png";
        public decimal PrecoUnitario { get; set; }
        public int Quantidade { get; set; }
        public decimal Subtotal => PrecoUnitario * Quantidade;
    }

    public class VendaCreateDto
    {
        public List<LinhaVendaCreateDto> Linhas { get; set; } = new();
    }

    public class LinhaVendaCreateDto
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }

    public class VendaDto
    {
        public int Id { get; set; }
        public string Data { get; set; } = "";
        public string Estado { get; set; } = "";
        public decimal Total { get; set; }
        public List<LinhaVendaDto> Linhas { get; set; } = new();
    }

    public class LinhaVendaDto
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = "";
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal => PrecoUnitario * Quantidade;
    }
}
