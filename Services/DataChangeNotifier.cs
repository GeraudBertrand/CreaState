using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CreaState.Services
{
    // Signale toute écriture EF réussie (pages, API, services) pour rafraîchir les vues temps réel comme le Dashboard
    public class DataChangeNotifier : SaveChangesInterceptor
    {
        public event Action? OnChange;

        public void Notify() => OnChange?.Invoke();

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            if (result > 0) Notify();
            return base.SavedChanges(eventData, result);
        }

        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            if (result > 0) Notify();
            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
