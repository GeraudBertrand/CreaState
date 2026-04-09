using CreaState.Data;
using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Repositories.Implementations
{
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        public RoleRepository(AppDbContext db) : base(db) { }

        public async Task<Role?> GetWithPermissionsAsync(int id)
            => await _dbSet
                .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);

        public async Task<List<Role>> GetAllWithPermissionsAsync()
            => await _dbSet
                .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .ToListAsync();

        public async Task<Role?> GetByNameAsync(string name)
            => await _dbSet.FirstOrDefaultAsync(r => r.Name == name);

        public async Task<Role?> GetDefaultRoleAsync()
            => await _dbSet.FirstOrDefaultAsync(r => r.IsDefault);
    }
}
