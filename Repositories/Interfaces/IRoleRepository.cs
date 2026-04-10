using CreaState.Models;

namespace CreaState.Repositories.Interfaces
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role?> GetWithPermissionsAsync(int id);
        Task<List<Role>> GetAllWithPermissionsAsync();
        Task<Role?> GetByNameAsync(string name);
        Task<Role?> GetDefaultRoleAsync();
    }
}
