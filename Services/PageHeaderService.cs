namespace CreaState.Services
{
    public class PageHeaderService
    {
        public string Title { get; private set; } = "Créalab";
        public string Description { get; private set; } = "";

        // L'événement pour dire au Layout de se rafraîchir
        public event Action? OnChange;

        // La méthode que tes pages (Admin, Dashboard) appelleront
        public void SetTitle(string title, string description = "")
        {
            Title = title;
            Description = description;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
