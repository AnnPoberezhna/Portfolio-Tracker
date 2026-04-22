using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace PortfolioTracker.Models
{
    public class UserActivity
    {
        [Key]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = string.Empty;

        public IdentityUser? User { get; set; }

        public DateTime RegisteredAtUtc { get; set; }

        public DateTime? LastSeenUtc { get; set; }
    }
}
