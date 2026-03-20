using CreaState.Data;
using CreaState.Models;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Services
{
    public class RoleService
    {
        private readonly AppDbContext _db;

        public RoleService(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Récupère tous les rôles avec leurs permissions.
        /// </summary>
        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _db.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .OrderBy(r => r.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère un rôle par son ID avec ses permissions.
        /// </summary>
        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            return await _db.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
