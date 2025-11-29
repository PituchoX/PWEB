namespace RESTfulAPIPWeb.Entities
{
    public class Venda
    {
        public int Id { get; set; }

        public DateTime Data { get; set; }

        public string Estado { get; set; } = "Pendente";

        // Ligação ao Cliente
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        // Linhas da Venda (OBRIGATÓRIO)
        public ICollection<LinhaVenda>? LinhasVenda { get; set; }
    }
}
