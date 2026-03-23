namespace CreaState.Models
{
    public class MemberRole
    {
        public int MemberId { get; set; }
        public Member? Member { get; set; }

        public int RoleId { get; set; }
        public Role? Role { get; set; }
    }
}
