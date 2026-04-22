using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Data;
using PortfolioTracker.Models;

namespace PortfolioTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Users()
        {
            var currentUserId = _userManager.GetUserId(User);
            var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
            var activities = new Dictionary<string, UserActivity>();

            var pageModel = new AdminUsersPageViewModel();

            var utcNow = DateTime.UtcNow;
            var today = utcNow.Date;
            var tomorrow = today.AddDays(1);

            pageModel.TotalUsers = users.Count;

            try
            {
                var userIds = users.Select(u => u.Id).ToList();
                activities = await _context.UserActivities
                    .Where(a => userIds.Contains(a.UserId))
                    .ToDictionaryAsync(a => a.UserId);

                pageModel.NewUsersToday = await _context.UserActivities
                    .CountAsync(a => a.RegisteredAtUtc >= today && a.RegisteredAtUtc < tomorrow);
                pageModel.ActiveUsersToday = await _context.UserActivities
                    .CountAsync(a => a.LastSeenUtc.HasValue && a.LastSeenUtc.Value >= today && a.LastSeenUtc.Value < tomorrow);
            }
            catch (SqliteException ex) when (ex.Message.Contains("no such table: UserActivities", StringComparison.OrdinalIgnoreCase))
            {
                TempData["AdminStatusMessage"] = "User activity tracking table is missing. Run database migration to enable daily metrics.";
                pageModel.NewUsersToday = 0;
                pageModel.ActiveUsersToday = 0;
            }

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                activities.TryGetValue(user.Id, out var activity);

                pageModel.Users.Add(new UserManagementViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    Roles = roles,
                    RegisteredAtUtc = activity?.RegisteredAtUtc,
                    LastSeenUtc = activity?.LastSeenUtc,
                    LockoutEnd = user.LockoutEnd,
                    IsLockedOut = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                    IsCurrentUser = user.Id == currentUserId
                });
            }

            return View(pageModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string userId, int days = 1)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == userId)
            {
                TempData["AdminStatusMessage"] = "You cannot block your own account.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["AdminStatusMessage"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            await _userManager.SetLockoutEnabledAsync(user, true);
            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddDays(Math.Max(days, 1)));

            TempData["AdminStatusMessage"] = result.Succeeded
                ? $"{user.Email} blocked for {Math.Max(days, 1)} day(s)."
                : "Could not block selected user.";

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUserPermanently(string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == userId)
            {
                TempData["AdminStatusMessage"] = "You cannot block your own account.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["AdminStatusMessage"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            await _userManager.SetLockoutEnabledAsync(user, true);
            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

            TempData["AdminStatusMessage"] = result.Succeeded
                ? $"{user.Email} has been permanently blocked."
                : "Could not block selected user.";

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["AdminStatusMessage"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            var result = await _userManager.SetLockoutEndDateAsync(user, null);

            TempData["AdminStatusMessage"] = result.Succeeded
                ? $"{user.Email} is now unblocked."
                : "Could not unblock selected user.";

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["AdminStatusMessage"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var result = await _userManager.AddToRoleAsync(user, "Admin");
                if (!result.Succeeded)
                {
                    TempData["AdminStatusMessage"] = "Could not assign Admin role.";
                    return RedirectToAction(nameof(Users));
                }
            }

            TempData["AdminStatusMessage"] = $"{user.Email} is now an Admin.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DemoteFromAdmin(string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == userId)
            {
                TempData["AdminStatusMessage"] = "You cannot remove your own Admin role.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["AdminStatusMessage"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    TempData["AdminStatusMessage"] = "At least one Admin account must remain.";
                    return RedirectToAction(nameof(Users));
                }

                var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
                if (!result.Succeeded)
                {
                    TempData["AdminStatusMessage"] = "Could not remove Admin role.";
                    return RedirectToAction(nameof(Users));
                }
            }

            TempData["AdminStatusMessage"] = $"Admin role removed from {user.Email}.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == userId)
            {
                TempData["AdminStatusMessage"] = "You cannot delete your own account from this panel.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["AdminStatusMessage"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    TempData["AdminStatusMessage"] = "At least one Admin account must remain.";
                    return RedirectToAction(nameof(Users));
                }
            }

            var result = await _userManager.DeleteAsync(user);
            TempData["AdminStatusMessage"] = result.Succeeded
                ? $"User {user.Email} has been deleted."
                : "Could not delete selected user.";

            return RedirectToAction(nameof(Users));
        }
    }
}
