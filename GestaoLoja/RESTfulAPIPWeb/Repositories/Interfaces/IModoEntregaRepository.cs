using RESTfulAPI.Entities;

namespace RESTfulAPI.Repositories.Interfaces
{
    public interface IModoEntregaRepository
    {
        Task<List<ModoEntrega>> GetAllAsync();
        Task<ModoEntrega?> GetByIdAsync(int id);
        Task<ModoEntrega> AddAsync(ModoEntrega modo);
        Task<bool> UpdateAsync(ModoEntrega modo);
        Task<bool> DeleteAsync(int id);
    }
}
