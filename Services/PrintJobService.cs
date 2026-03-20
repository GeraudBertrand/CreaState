using CreaState.Data;
using CreaState.Models;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Services
{
    public class PrintJobService
    {
        private readonly AppDbContext _db;

        public PrintJobService(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Récupère les impressions récentes avec filtres optionnels.
        /// </summary>
        public async Task<List<PrintJob>> GetRecentJobsAsync(
            int days = 30, string? printerId = null, PrintStatus? status = null)
        {
            var since = DateTime.UtcNow.AddDays(-days);

            var query = _db.PrintJobs
                .Include(pj => pj.Printer)
                .Where(pj => pj.StartTime >= since)
                .AsQueryable();

            if (!string.IsNullOrEmpty(printerId))
                query = query.Where(pj => pj.PrinterId == printerId);

            if (status.HasValue)
                query = query.Where(pj => pj.Status == status.Value);

            return await query
                .OrderByDescending(pj => pj.StartTime)
                .ToListAsync();
        }
    }
}
