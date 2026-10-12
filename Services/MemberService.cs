using CreaState.Data;
using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Services
{
    public class MemberService
    {
        private readonly IMembreRepository _membreRepo;
        private readonly AppDbContext _db;
        private readonly DataChangeNotifier _notifier;

        public MemberService(IMembreRepository membreRepo, AppDbContext db, DataChangeNotifier notifier)
        {
            _membreRepo = membreRepo;
            _db = db;
            _notifier = notifier;
        }

        public async Task<Membre?> GetMemberByIdAsync(int id)
            => await _membreRepo.GetWithRolesAsync(id);

        public async Task<List<Membre>> GetAllMembersAsync()
            => await _membreRepo.GetAllWithRolesAsync();

        public async Task<List<Membre>> GetActiveMembersAsync()
            => await _membreRepo.GetAllActiveAsync();

        // Tous les comptes (élèves inclus) : _db.Membres ne renvoie que UserType == Membre (TPH)
        public async Task<List<User>> GetAllUsersAsync()
            => await _db.Users.AsNoTracking()
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role!)
                .ToListAsync();

        public async Task<bool> UpdateMemberRolesAsync(int membreId, List<int> roleIds)
        {
            if (!await _db.Users.AnyAsync(u => u.Id == membreId)) return false;

            var validRoles = await _db.Roles.Where(r => roleIds.Contains(r.Id)).ToListAsync();
            if (validRoles.Count != roleIds.Count) return false;

            // Remove existing roles via the UserRoles join table
            var existingUserRoles = _db.Set<AppUserRole>().Where(ur => ur.UserId == membreId);
            _db.Set<AppUserRole>().RemoveRange(existingUserRoles);

            foreach (var roleId in roleIds)
                _db.Set<AppUserRole>().Add(new AppUserRole { UserId = membreId, RoleId = roleId });

            await _db.SaveChangesAsync();

            // Un rôle non par défaut fait du compte un Membre ; EF ne change pas le type d'une entité suivie, d'où le SQL direct.
            // Les colonnes propres à Membre sont NULL pour un élève : on les remplit, sinon la lecture du Membre plante.
            var userType = validRoles.Any(r => !r.IsDefault) ? UserType.Membre : UserType.Eleve;
            await _db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE Users SET UserType = {(int)userType},
                    IsActive = COALESCE(IsActive, TRUE), JoinDate = COALESCE(JoinDate, {DateTime.UtcNow})
                WHERE Id = {membreId}");
            _notifier.Notify(); // ExecuteUpdate passe à côté de l'interceptor SaveChanges
            return true;
        }

        public async Task<bool> RemoveMemberAsync(int membreId)
        {
            var membre = await _membreRepo.GetByIdAsync(membreId);
            if (membre == null) return false;

            await _membreRepo.DeleteAsync(membre);
            return true;
        }
    }
}
