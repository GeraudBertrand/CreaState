using CreaState.Data;
using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Repositories.Implementations
{
    public class EvenementRepository : Repository<Evenement>, IEvenementRepository
    {
        public EvenementRepository(AppDbContext db) : base(db) { }

        public async Task<List<Evenement>> GetUpcomingAsync()
            => await _dbSet
                .Where(e => e.Date >= DateTime.Now)
                .OrderBy(e => e.Date)
                .ToListAsync();
    }
}
