using CreaState.Data;
using CreaState.Models;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Services
{
    public class MaintenanceService
    {
        private readonly AppDbContext _db;

        public MaintenanceService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<MaintenanceRecord>> GetRecordsForPrinterAsync(string printerId)
        {
            return await _db.MaintenanceRecords
                .Include(m => m.PerformedBy)
                .Where(m => m.PrinterId == printerId)
                .OrderByDescending(m => m.PerformedAt)
                .ToListAsync();
        }

        public async Task<List<MaintenanceRecord>> GetAllRecordsAsync()
        {
            return await _db.MaintenanceRecords
                .Include(m => m.Printer)
                .Include(m => m.PerformedBy)
                .OrderByDescending(m => m.PerformedAt)
                .ToListAsync();
        }

        public async Task<List<MaintenanceRecord>> GetUpcomingMaintenanceAsync()
        {
            var deadline = DateTime.UtcNow.AddDays(7);
            return await _db.MaintenanceRecords
                .Include(m => m.Printer)
                .Where(m => m.NextScheduledAt != null && m.NextScheduledAt <= deadline)
                .OrderBy(m => m.NextScheduledAt)
                .ToListAsync();
        }

        public async Task<List<MaintenanceRecord>> GetBreakdownReportsAsync()
        {
            return await _db.MaintenanceRecords
                .Include(m => m.Printer)
                .Include(m => m.PerformedBy)
                .Where(m => m.IsBreakdownReport)
                .OrderByDescending(m => m.PerformedAt)
                .ToListAsync();
        }

        public async Task<MaintenanceRecord> AddRecordAsync(MaintenanceRecord record)
        {
            _db.MaintenanceRecords.Add(record);
            await _db.SaveChangesAsync();
            return record;
        }

        public async Task<MaintenanceRecord> ReportBreakdownAsync(string printerId, int reporterMemberId, string description)
        {
            var record = new MaintenanceRecord
            {
                PrinterId = printerId,
                PerformedByMemberId = reporterMemberId,
                Type = MaintenanceType.Repair,
                Description = description,
                PerformedAt = DateTime.UtcNow,
                IsBreakdownReport = true
            };

            _db.MaintenanceRecords.Add(record);
            await _db.SaveChangesAsync();
            return record;
        }

        public async Task<bool> ResolveBreakdownAsync(int recordId, int resolvedByMemberId, string description, MaintenanceType repairType)
        {
            var record = await _db.MaintenanceRecords.FindAsync(recordId);
            if (record == null || !record.IsBreakdownReport || record.IsResolved) return false;

            record.IsResolved = true;
            record.ResolvedAt = DateTime.UtcNow;
            record.ResolvedByMemberId = resolvedByMemberId;

            // Log the repair as a new maintenance record
            var repairRecord = new MaintenanceRecord
            {
                PrinterId = record.PrinterId,
                PerformedByMemberId = resolvedByMemberId,
                Type = repairType,
                Description = description,
                PerformedAt = DateTime.UtcNow,
                IsBreakdownReport = false
            };

            _db.MaintenanceRecords.Add(repairRecord);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<MaintenanceRecord>> GetUnresolvedBreakdownsAsync()
        {
            return await _db.MaintenanceRecords
                .Include(m => m.Printer)
                .Include(m => m.PerformedBy)
                .Where(m => m.IsBreakdownReport && !m.IsResolved)
                .OrderByDescending(m => m.PerformedAt)
                .ToListAsync();
        }
    }
}
