using GestaoLoja.Data;

namespace GestaoLoja.Entities
{
    public class Fornecedor
    {
        public int Id { get; set; }

        // Ligação ao utilizador
        public string ApplicationUserId { get; set; } = default!;
        public ApplicationUser? ApplicationUser { get; set; }

        // Dados próprios do fornecedor
        public string NomeEmpresa { get; set; } = "";

        // Estado do registo (Pendente / Aprovado)
        public string Estado { get; set; } = "Pendente";

        // Produtos deste fornecedor
        public ICollection<Produtos>? Produtos { get; set; }
    }
}
