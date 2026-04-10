using CreaState.Models;
using CreaState.Repositories.Interfaces;

namespace CreaState.Services
{
    public class RoleService
    {
        private readonly IRoleRepository _roleRepo;

        public RoleService(IRoleRepository roleRepo)
        {
            _roleRepo = roleRepo;
        }

        public async Task<List<Role>> GetAllRolesAsync()
            => await _roleRepo.GetAllWithPermissionsAsync();

        public async Task<Role?> GetRoleByIdAsync(int id)
            => await _roleRepo.GetWithPermissionsAsync(id);
    }
}
