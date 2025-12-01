namespace GestaoLoja.Entities
{
    public class Produtos
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public decimal PrecoBase { get; set; }
        public decimal Percentagem { get; set; }
        public decimal PrecoFinal { get; set; }

        public string Estado { get; set; } = "Pendente";

        public int Stock { get; set; }

        public string Imagem { get; set; } = "semfoto.png";

        public int CategoriaId { get; set; }
        public Categorias? Categoria { get; set; }

        public int ModoEntregaId { get; set; }
        public ModoEntrega? ModoEntrega { get; set; }

        public int FornecedorId { get; set; }
        public Fornecedor? Fornecedor { get; set; }

    }
}
