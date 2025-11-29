namespace RESTfulAPIPWeb.Entities
{
    public class LinhaVenda
    {
        public int Id { get; set; }

        public int VendaId { get; set; }
        public Venda? Venda { get; set; }

        public int ProdutoId { get; set; }
        public Produto? Produto { get; set; }

        public int Quantidade { get; set; }

        // Preço aplicado no momento da compra (obrigatório no enunciado)
        public decimal Preco { get; set; }
    }
}
