using CreaState.Data;
using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Repositories.Implementations
{
    public class PrintJobRepository : Repository<PrintJob>, IPrintJobRepository
    {
        public PrintJobRepository(AppDbContext db) : base(db) { }

        public async Task<List<PrintJob>> GetRecentAsync(int days = 30, int? printerId = null, PrintStatus? status = null)
        {
            var since = DateTime.UtcNow.AddDays(-days);

            var query = _dbSet
                .Include(pj => pj.Printer)
                .Where(pj => pj.StartTime >= since)
                .AsQueryable();

            if (printerId.HasValue)
                query = query.Where(pj => pj.PrinterId == printerId.Value);

            if (status.HasValue)
                query = query.Where(pj => pj.Status == status.Value);

            return await query.OrderByDescending(pj => pj.StartTime).ToListAsync();
        }
    }
}
