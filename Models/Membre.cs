namespace CreaState.Models
{
    public class Membre : User
    {
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<MembreRole> MembreRoles { get; set; } = [];
    }
}
