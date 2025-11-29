using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Entities;
using RESTfulAPIPWeb.Repositories.Interfaces;

namespace RESTfulAPIPWeb.Repositories.Services
{
    public class ModoEntregaRepository : IModoEntregaRepository
    {
        private readonly AppDbContext _db;

        public ModoEntregaRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<ModoEntrega>> GetAllAsync()
        {
            return await _db.ModosEntrega.ToListAsync();
        }

        public async Task<ModoEntrega?> GetByIdAsync(int id)
        {
            return await _db.ModosEntrega.FindAsync(id);
        }

        public async Task<ModoEntrega> AddAsync(ModoEntrega modo)
        {
            _db.ModosEntrega.Add(modo);
            await _db.SaveChangesAsync();
            return modo;
        }

        public async Task<bool> UpdateAsync(ModoEntrega modo)
        {
            _db.ModosEntrega.Update(modo);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var modo = await _db.ModosEntrega.FindAsync(id);
            if (modo == null)
                return false;

            _db.ModosEntrega.Remove(modo);
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
