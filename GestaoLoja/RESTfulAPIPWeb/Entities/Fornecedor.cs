namespace RESTfulAPIPWeb.Entities
{
    public class Fornecedor
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } = default!;
        public ApplicationUser? ApplicationUser { get; set; }

        public string NomeEmpresa { get; set; } = "";
        public string NIF { get; set; } = "";

        public string Estado { get; set; } = "Pendente";

        public ICollection<Produto>? Produtos { get; set; }
        public ICollection<Venda>? Vendas { get; set; }
    }
}
