using CreaState.Models;

namespace CreaState.Repositories.Interfaces
{
    public interface IPrinterRepository : IRepository<Printer>
    {
        Task<List<Printer>> GetEnabledAsync();
    }
}
