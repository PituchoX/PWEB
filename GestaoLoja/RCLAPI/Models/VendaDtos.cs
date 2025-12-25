namespace RCLAPI.Models
{
    public class ItemCarrinhoDto
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = "";
        public string ProdutoImagem { get; set; } = "semfoto.png";
        public decimal PrecoUnitario { get; set; }
        public int Quantidade { get; set; }
        public decimal Subtotal => PrecoUnitario * Quantidade;
    }

    public class VendaCreateDto
    {
        public List<LinhaVendaCreateDto> Linhas { get; set; } = new();
    }

    public class LinhaVendaCreateDto
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }

    public class VendaDto
    {
        public int Id { get; set; }
        public string Data { get; set; } = "";
        public string Estado { get; set; } = "";
        public decimal Total { get; set; }
        public List<LinhaVendaDto> Linhas { get; set; } = new();
    }

    public class LinhaVendaDto
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = "";
        public string ProdutoImagem { get; set; } = "semfoto.png";
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal => PrecoUnitario * Quantidade;
    }

    // Resposta da API ao criar venda
    public class VendaCriadaResponseDto
    {
        public int VendaId { get; set; }
        public decimal Total { get; set; }
        public string? Estado { get; set; }
        public string? Message { get; set; }
    }

    // Resposta da simulação de pagamento
    public class PagamentoResponseDto
    {
        public string? Message { get; set; }
        public int VendaId { get; set; }
        public decimal Total { get; set; }
        public string? MetodoPagamento { get; set; }
        public DateTime DataPagamento { get; set; }
    }

    // DTO para vendas do fornecedor
    public class VendaFornecedorDto
    {
        public int? VendaId { get; set; }
        public string? Data { get; set; }
        public string? Estado { get; set; }
        public string? ProdutoNome { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Total { get; set; }
        public string? ClienteNome { get; set; }
    }
}
