
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // Display all registered users
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();

        var userDetails = new List<object>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            userDetails.Add(new
            {
                UserId = user.Id,
                Email = user.Email,
                Roles = string.Join(", ", roles)
            });
        }

        ViewBag.Users = userDetails;

        return View();
    }

    // Assign a role to a user
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        if (!await _roleManager.RoleExistsAsync(role))
        {
            TempData["Error"] = "Invalid role selected.";
            return RedirectToAction(nameof(Index));
        }

        var allowedRoles = new[] { "Admin", "Manager", "SalesExecutive" };

        if (!allowedRoles.Contains(role))
        {
            return BadRequest();
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (!removeResult.Succeeded)
        {
            TempData["Error"] = "Could not update the user's role.";
            return RedirectToAction(nameof(Index));
        }

        var addResult = await _userManager.AddToRoleAsync(user, role);

        TempData[addResult.Succeeded ? "Success" : "Error"] =
            addResult.Succeeded
                ? "User role updated successfully."
                : "Could not assign the selected role.";

        return RedirectToAction(nameof(Index));
    }
}
