namespace RESTfulAPIPWeb.Dtos
{
    public class LinhaVendaViewDto
    {
        public string ProdutoNome { get; set; } = "";
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }

    public class VendaViewDto
    {
        public int Id { get; set; }
        public string ClienteNome { get; set; } = "";
        public DateTime Data { get; set; }
        public string Estado { get; set; } = "";
        public decimal Total { get; set; }
        public List<LinhaVendaViewDto> Linhas { get; set; } = new();
    }
}
