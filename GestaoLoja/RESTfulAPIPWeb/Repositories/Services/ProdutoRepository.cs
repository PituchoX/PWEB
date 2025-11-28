using Microsoft.EntityFrameworkCore;
using RESTfulAPI.Data;
using RESTfulAPI.Entities;
using RESTfulAPI.Repositories.Interfaces;

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
                .ToListAsync();
        }

        public async Task<Produto?> GetByIdAsync(int id)
        {
            return await _db.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
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
            _db.Produtos.Update(produto);
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
