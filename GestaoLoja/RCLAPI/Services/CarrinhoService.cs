using RCLAPI.Models;

namespace RCLAPI.Services
{
    public class CarrinhoService
    {
        private readonly List<ItemCarrinhoDto> _itens = new();

        public event Action? OnChange;

        public IReadOnlyList<ItemCarrinhoDto> Itens => _itens.AsReadOnly();

        public int TotalItens => _itens.Sum(i => i.Quantidade);

        public decimal Total => _itens.Sum(i => i.Subtotal);

        public void AdicionarItem(ProdutoDto produto, int quantidade = 1)
        {
            var item = _itens.FirstOrDefault(i => i.ProdutoId == produto.Id);
            if (item != null)
            {
                item.Quantidade += quantidade;
            }
            else
            {
                _itens.Add(new ItemCarrinhoDto
                {
                    ProdutoId = produto.Id,
                    ProdutoNome = produto.Nome,
                    ProdutoImagem = produto.Imagem,
                    PrecoUnitario = produto.PrecoFinal,
                    Quantidade = quantidade
                });
            }
            NotifyStateChanged();
        }

        public void RemoverItem(int produtoId)
        {
            var item = _itens.FirstOrDefault(i => i.ProdutoId == produtoId);
            if (item != null)
            {
                _itens.Remove(item);
                NotifyStateChanged();
            }
        }

        public void AtualizarQuantidade(int produtoId, int quantidade)
        {
            var item = _itens.FirstOrDefault(i => i.ProdutoId == produtoId);
            if (item != null)
            {
                if (quantidade <= 0)
                {
                    _itens.Remove(item);
                }
                else
                {
                    item.Quantidade = quantidade;
                }
                NotifyStateChanged();
            }
        }

        public void LimparCarrinho()
        {
            _itens.Clear();
            NotifyStateChanged();
        }

        public VendaCreateDto GetVendaDto()
        {
            return new VendaCreateDto
            {
                Linhas = _itens.Select(i => new LinhaVendaCreateDto
                {
                    ProdutoId = i.ProdutoId,
                    Quantidade = i.Quantidade
                }).ToList()
            };
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
