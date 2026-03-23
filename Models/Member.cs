using System.ComponentModel.DataAnnotations.Schema;

namespace CreaState.Models
{
    public class Member : User
    {
        public ICollection<MemberRole> MemberRoles { get; set; } = [];

        public ClassYearEnum ClassYear { get; set; } = ClassYearEnum.Other;

        public bool IsActive { get; set; } = true;

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public IEnumerable<Role> Roles => MemberRoles.Select(mr => mr.Role!);

        [NotMapped]
        public bool IsBoardMember => MemberRoles.Any(mr => mr.Role?.Name != "Eleve");

        [NotMapped]
        public string AvatarColor => IsBoardMember ? "var(--accent-magenta)" : "var(--primary-blue)";

        [NotMapped]
        public string RoleLabel => string.Join(", ", Roles.Select(r => r.DisplayName));

        [NotMapped]
        public string ClassYearLabel => ClassYear.GetDisplayName();
    }
}
