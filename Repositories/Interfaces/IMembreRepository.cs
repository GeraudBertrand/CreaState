using CreaState.Models;

namespace CreaState.Repositories.Interfaces
{
    public interface IMembreRepository : IRepository<Membre>
    {
        Task<Membre?> GetWithRolesAsync(int id);
        Task<Membre?> GetByEmailWithRolesAsync(string email);
        Task<List<Membre>> GetAllActiveAsync();
        Task<List<Membre>> GetAllWithRolesAsync();
    }
}
