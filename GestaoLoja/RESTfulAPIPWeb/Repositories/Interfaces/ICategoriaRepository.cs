using RESTfulAPIPWeb.Entities;

namespace RESTfulAPIPWeb.Repositories.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<List<Categoria>> GetAllAsync();
        Task<Categoria?> GetByIdAsync(int id);
        Task<Categoria> AddAsync(Categoria categoria);
        Task<bool> UpdateAsync(Categoria categoria);
        Task<bool> DeleteAsync(int id);
    }
}
