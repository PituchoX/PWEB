using GestaoLoja.Data;

namespace GestaoLoja.Entities
{
    public class Vendas
    {
        public int Id { get; set; }

        public string Data { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Estados: Pendente / Confirmada / Rejeitada / Expedida
        public string Estado { get; set; } = "Pendente";

        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public ICollection<LinhasVenda>? LinhasVenda { get; set; }
    }
}
