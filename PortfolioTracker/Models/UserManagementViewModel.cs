namespace PortfolioTracker.Models
{
    public class UserManagementViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public DateTime? RegisteredAtUtc { get; set; }
        public DateTime? LastSeenUtc { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool IsLockedOut { get; set; }
        public bool IsCurrentUser { get; set; }
    }

    public class AdminUsersPageViewModel
    {
        public int TotalUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int ActiveUsersToday { get; set; }
        public List<UserManagementViewModel> Users { get; set; } = new();
    }
}
