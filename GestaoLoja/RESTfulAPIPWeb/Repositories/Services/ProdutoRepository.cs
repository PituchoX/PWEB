using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Entities;
using RESTfulAPIPWeb.Repositories.Interfaces;

namespace RESTfulAPI.Repositories.Services
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly AppDbContext _db;

        public ProdutoRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Produto>> GetAllAsync()
        {
            return await _db.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .Include(p => p.Fornecedor)
                .ToListAsync();
        }

        public async Task<Produto?> GetByIdAsync(int id)
        {
            return await _db.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .Include(p => p.Fornecedor)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Produto> AddAsync(Produto produto)
        {
            _db.Produtos.Add(produto);
            await _db.SaveChangesAsync();
            return produto;
        }

        public async Task<bool> UpdateAsync(Produto produto)
        {
            var existing = await _db.Produtos.FindAsync(produto.Id);
            if (existing == null)
                return false;

            existing.Nome = produto.Nome;
            existing.PrecoBase = produto.PrecoBase;
            existing.Percentagem = produto.Percentagem;
            existing.PrecoFinal = produto.PrecoFinal;
            existing.Estado = produto.Estado;
            existing.Stock = produto.Stock;
            existing.Imagem = produto.Imagem;
            existing.CategoriaId = produto.CategoriaId;
            existing.ModoEntregaId = produto.ModoEntregaId;
            existing.FornecedorId = produto.FornecedorId;

            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var prod = await _db.Produtos.FindAsync(id);
            if (prod == null)
                return false;

            _db.Produtos.Remove(prod);
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
