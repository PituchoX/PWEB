namespace GestaoLoja.Entities
{
    public class Produtos
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        // preço definido pelo fornecedor
        public decimal PrecoBase { get; set; }

        // percentagem definida pelo funcionário/admin
        public decimal Percentagem { get; set; }

        // preço final = precobase + (percentagem %)
        public decimal PrecoFinal { get; set; }

        // estados: Pendente / Activo / Inactivo
        public string Estado { get; set; } = "Pendente";

        // stock disponível
        public int Stock { get; set; }

        // nome do ficheiro da imagem
        public string Imagem { get; set; } = "semfoto.png";

        // relações
        public int CategoriaId { get; set; }
        public Categorias Categoria { get; set; }

        public int ModoEntregaId { get; set; }
        public ModoEntrega ModoEntrega { get; set; }
    }
}
