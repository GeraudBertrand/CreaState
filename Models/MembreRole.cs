namespace CreaState.Models
{
    public class MembreRole
    {
        public int MembreId { get; set; }
        public Membre? Membre { get; set; }

        public int RoleId { get; set; }
        public Role? Role { get; set; }
    }
}
