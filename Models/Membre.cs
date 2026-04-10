namespace CreaState.Models
{
    public class Membre : User
    {
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Les rôles sont hérités de User.UserRoles (via AppUserRole / Identity)
    }
}
