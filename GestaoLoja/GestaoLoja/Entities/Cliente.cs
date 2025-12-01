using GestaoLoja.Data;

namespace GestaoLoja.Entities
{
    public class Cliente
    {
        public int Id { get; set; }

        // Ligação ao utilizador Identity
        public string ApplicationUserId { get; set; } = default!;
        public ApplicationUser? ApplicationUser { get; set; }

        // Dados próprios do cliente (exceto Nome)
        public string NIF { get; set; } = "";
        public string Estado { get; set; } = "Pendente";

        // Relação com vendas
        public ICollection<Vendas>? Vendas { get; set; }
    }
}
