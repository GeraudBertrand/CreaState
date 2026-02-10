namespace CreaState.Models
{
    public class Member : User
    {
        public RoleEnum Role { get; set; } = RoleEnum.Member;

        public ClassYearEnum ClassYear { get; set; } = ClassYearEnum.Other;

        public bool IsBoardMember => Role != RoleEnum.Member;

        public string AvatarColor => IsBoardMember ? "var(--accent-magenta)" : "var(--primary-blue)";

        public string RoleLabel => Role.GetDisplayName();
        public string ClassYearLabel => ClassYear.GetDisplayName();
    }
}
