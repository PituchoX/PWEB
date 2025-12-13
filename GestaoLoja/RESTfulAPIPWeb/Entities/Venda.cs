namespace RESTfulAPIPWeb.Entities
{
    public class Venda
    {
        public int Id { get; set; }

        public string Data { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public string Estado { get; set; } = "Pendente";

        // Ligação ao Cliente
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        // Linhas da Venda
        public ICollection<LinhaVenda>? LinhasVenda { get; set; }
    }
}
