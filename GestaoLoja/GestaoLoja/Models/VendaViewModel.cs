namespace GestaoLoja.Models
{
    public class VendaViewModel
    {
        public int Id { get; set; }
        public string ClienteNome { get; set; } = "";
        public DateTime Data { get; set; }
        public string Estado { get; set; } = "";
        public decimal Total { get; set; }
        public List<LinhaVendaViewModel> Linhas { get; set; } = new();
    }

    public class LinhaVendaViewModel
    {
        public string ProdutoNome { get; set; } = "";
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }
}
