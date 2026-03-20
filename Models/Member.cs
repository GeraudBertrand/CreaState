using System.ComponentModel.DataAnnotations.Schema;

namespace CreaState.Models
{
    public class Member : User
    {
        public int RoleId { get; set; }
        public Role? Role { get; set; }

        public ClassYearEnum ClassYear { get; set; } = ClassYearEnum.Other;

        public bool IsActive { get; set; } = true;

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public bool IsBoardMember => RoleId != 1;

        [NotMapped]
        public string AvatarColor => IsBoardMember ? "var(--accent-magenta)" : "var(--primary-blue)";

        [NotMapped]
        public string RoleLabel => Role?.DisplayName ?? "";

        [NotMapped]
        public string ClassYearLabel => ClassYear.GetDisplayName();
    }
}
