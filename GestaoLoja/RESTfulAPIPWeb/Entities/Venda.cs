namespace RESTfulAPIPWeb.Entities
{
    public class Venda
    {
        public int Id { get; set; }

        public DateTime Data { get; set; } = DateTime.Now;

        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        // depois o funcionário/admin confirma
        public string Estado { get; set; } = "Pendente";
        // Pendente, Confirmada, Rejeitada, Expedida

        public ICollection<LinhaVenda>? Linhas { get; set; }
    }
}
