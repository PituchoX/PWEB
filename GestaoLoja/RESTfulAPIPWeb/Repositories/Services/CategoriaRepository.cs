using Microsoft.EntityFrameworkCore;
using RESTfulAPI.Data;
using RESTfulAPIPWeb.Entities;
using RESTfulAPIPWeb.Repositories.Interfaces;

namespace RESTfulAPIPWeb.Repositories.Services
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _db;

        public CategoriaRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Categoria>> GetAllAsync() =>
            await _db.Categorias.ToListAsync();

        public async Task<Categoria?> GetByIdAsync(int id) =>
            await _db.Categorias.FindAsync(id);

        public async Task<Categoria> AddAsync(Categoria categoria)
        {
            _db.Categorias.Add(categoria);
            await _db.SaveChangesAsync();
            return categoria;
        }

        public async Task<bool> UpdateAsync(Categoria categoria)
        {
            _db.Categorias.Update(categoria);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cat = await _db.Categorias.FindAsync(id);
            if (cat == null) return false;

            _db.Categorias.Remove(cat);
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
