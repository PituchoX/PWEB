using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RESTfulAPIPWeb.Entities
{
    public class Cliente
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } = default!;
        public ApplicationUser? ApplicationUser { get; set; }

        public string NIF { get; set; } = "";

        public string Estado { get; set; } = "Pendente";
        // Pendente, Ativo, Inativo

        public ICollection<Venda>? Vendas { get; set; }
    }
}
