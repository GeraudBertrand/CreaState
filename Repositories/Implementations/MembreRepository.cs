using CreaState.Data;
using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Repositories.Implementations
{
    public class MembreRepository : Repository<Membre>, IMembreRepository
    {
        public MembreRepository(AppDbContext db) : base(db) { }

        public async Task<Membre?> GetWithRolesAsync(int id)
            => await _dbSet
                .Include(m => m.UserRoles).ThenInclude(ur => ur.Role!)
                    .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(m => m.Id == id);

        public async Task<Membre?> GetByEmailWithRolesAsync(string email)
            => await _dbSet
                .Include(m => m.UserRoles).ThenInclude(ur => ur.Role!)
                    .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(m => m.Email == email);

        public async Task<List<Membre>> GetAllActiveAsync()
            => await _dbSet
                .Include(m => m.UserRoles).ThenInclude(ur => ur.Role!)
                .Where(m => m.IsActive)
                .ToListAsync();

        public async Task<List<Membre>> GetAllWithRolesAsync()
            => await _dbSet
                .Include(m => m.UserRoles).ThenInclude(ur => ur.Role!)
                .ToListAsync();
    }
}
