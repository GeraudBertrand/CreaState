using CreaState.Models;
using CreaState.Repositories.Interfaces;

namespace CreaState.Services
{
    public class MaintenanceService
    {
        private readonly IMaintenanceRepository _maintenanceRepo;

        public MaintenanceService(IMaintenanceRepository maintenanceRepo)
        {
            _maintenanceRepo = maintenanceRepo;
        }

        public async Task<List<Models.Maintenance>> GetRecordsForPrinterAsync(int printerId)
            => await _maintenanceRepo.GetByPrinterIdAsync(printerId);

        public async Task<List<Models.Maintenance>> GetAllRecordsAsync()
            => await _maintenanceRepo.GetAllWithDetailsAsync();

        public async Task<Models.Maintenance> AddRecordAsync(Models.Maintenance record)
            => await _maintenanceRepo.AddAsync(record);
    }
}
