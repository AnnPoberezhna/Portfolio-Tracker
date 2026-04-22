using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Models;

namespace PortfolioTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Users()
        {
            var currentUserId = _userManager.GetUserId(User);
            var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
            var viewModel = new List<UserManagementViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                viewModel.Add(new UserManagementViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    Roles = roles,
                    IsCurrentUser = user.Id == currentUserId
                });
            }

            return View(viewModel);
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
