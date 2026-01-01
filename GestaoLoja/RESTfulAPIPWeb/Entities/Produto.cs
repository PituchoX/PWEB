namespace RESTfulAPIPWeb.Entities
{
    public class Produto
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public decimal PrecoBase { get; set; }
        public decimal Percentagem { get; set; }
        public decimal PrecoFinal { get; set; }

        public string Estado { get; set; } = "Pendente";

        public int Stock { get; set; }

        public string Imagem { get; set; } = "semfoto.png";

        // Relação com Categoria
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        // Subcategoria (opcional)
        public int? SubcategoriaId { get; set; }
        public Subcategoria? Subcategoria { get; set; }

        // Relação com Modo de Entrega
        public int ModoEntregaId { get; set; }
        public ModoEntrega? ModoEntrega { get; set; }

        // Relação obrigatória com Fornecedor
        public int FornecedorId { get; set; }
        public Fornecedor? Fornecedor { get; set; }
    }
}
