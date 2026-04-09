using CreaState.Models;

namespace CreaState.Repositories.Interfaces
{
    public interface IPrintJobRepository : IRepository<PrintJob>
    {
        Task<List<PrintJob>> GetRecentAsync(int days = 30, int? printerId = null, PrintStatus? status = null);
    }
}
