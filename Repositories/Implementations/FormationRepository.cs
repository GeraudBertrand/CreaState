using CreaState.Data;
using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Repositories.Implementations
{
    public class FormationRepository : Repository<Formation>, IFormationRepository
    {
        public FormationRepository(AppDbContext db) : base(db) { }

        public async Task<List<Formation>> GetUpcomingAsync()
            => await _dbSet
                .Include(f => f.Instructeur)
                .Where(f => f.Date >= DateOnly.FromDateTime(DateTime.Now))
                .OrderBy(f => f.Date)
                .ToListAsync();

        public async Task<Formation?> GetWithInstructeurAsync(int id)
            => await _dbSet
                .Include(f => f.Instructeur)
                .FirstOrDefaultAsync(f => f.Id == id);
    }
}
