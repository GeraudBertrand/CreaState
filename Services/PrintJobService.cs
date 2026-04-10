using CreaState.Models;
using CreaState.Repositories.Interfaces;

namespace CreaState.Services
{
    public class PrintJobService
    {
        private readonly IPrintJobRepository _printJobRepo;

        public PrintJobService(IPrintJobRepository printJobRepo)
        {
            _printJobRepo = printJobRepo;
        }

        public async Task<List<PrintJob>> GetRecentJobsAsync(int days = 30, int? printerId = null, PrintStatus? status = null)
            => await _printJobRepo.GetRecentAsync(days, printerId, status);

        public async Task<PrintJob> AddJobAsync(PrintJob job)
            => await _printJobRepo.AddAsync(job);
    }
}
