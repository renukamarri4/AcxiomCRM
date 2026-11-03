
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AcxiomCRM.Controllers;

[Authorize]
public class LeadsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public LeadsController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private string? CurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private async Task LoadUsers(string? selectedUserId = null)
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        ViewBag.Users = new SelectList(
            users, "Id", "Email", selectedUserId);
    }

    private async Task<bool> IsValidAssignedUser(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        return await _userManager.FindByIdAsync(userId) != null;
    }

    private void ValidateLeadOptions(Lead lead)
    {
        string[] statuses =
            { "New", "Contacted", "Qualified", "Converted", "Lost" };

        string[] sources =
            { "Website", "Referral", "Social Media", "Email", "Other" };

        string[] priorities = { "Low", "Medium", "High" };

        if (!statuses.Contains(lead.Status))
            ModelState.AddModelError("Status", "Select a valid status.");

        if (!sources.Contains(lead.Source))
            ModelState.AddModelError("Source", "Select a valid source.");

        if (!priorities.Contains(lead.Priority))
            ModelState.AddModelError("Priority", "Select a valid priority.");
    }

    // LEAD LIST AND SEARCH
    [Authorize(Roles = "Admin,Manager,SalesExecutive")]
    public async Task<IActionResult> Index(string? search)
    {
        var leads = _context.Leads
            .Include(l => l.AssignedUser)
            .AsQueryable();

        if (User.IsInRole("SalesExecutive"))
        {
            var userId = CurrentUserId();
            leads = leads.Where(l => l.AssignedUserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            leads = leads.Where(l =>
                l.Name.Contains(search) ||
                l.Email.Contains(search) ||
                l.Company.Contains(search));
        }

        ViewBag.Search = search;

        return View(await leads.OrderByDescending(l => l.Id).ToListAsync());
    }

    // CREATE LEAD - GET
    [Authorize(Roles = "Admin,Manager,SalesExecutive")]
    public async Task<IActionResult> Create()
    {
        if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            await LoadUsers();

        return View();
    }

    // CREATE LEAD - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager,SalesExecutive")]
    public async Task<IActionResult> Create(Lead lead)
    {
        if (User.IsInRole("SalesExecutive"))
            lead.AssignedUserId = CurrentUserId();
        else if (!await IsValidAssignedUser(lead.AssignedUserId))
            ModelState.AddModelError("AssignedUserId", "Please select a valid user.");

        ValidateLeadOptions(lead);

        if (!ModelState.IsValid)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                await LoadUsers(lead.AssignedUserId);

            return View(lead);
        }

        _context.Leads.Add(lead);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Lead created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // EDIT LEAD - GET
    [Authorize(Roles = "Admin,Manager,SalesExecutive")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var lead = await _context.Leads.FindAsync(id);

        if (lead == null)
            return NotFound();

        if (User.IsInRole("SalesExecutive") &&
            lead.AssignedUserId != CurrentUserId())
            return Forbid();

        if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            await LoadUsers(lead.AssignedUserId);

        return View(lead);
    }

    // EDIT LEAD - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager,SalesExecutive")]
    public async Task<IActionResult> Edit(int id, Lead lead)
    {
        if (id != lead.Id)
            return NotFound();

        var existingLead = await _context.Leads.FindAsync(id);

        if (existingLead == null)
            return NotFound();

        if (User.IsInRole("SalesExecutive"))
        {
            if (existingLead.AssignedUserId != CurrentUserId())
                return Forbid();
        }
        else if (!await IsValidAssignedUser(lead.AssignedUserId))
        {
            ModelState.AddModelError("AssignedUserId", "Please select a valid user.");
        }

        ValidateLeadOptions(lead);

        if (!ModelState.IsValid)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                await LoadUsers(lead.AssignedUserId);

            return View(lead);
        }

        existingLead.Name = lead.Name;
        existingLead.Email = lead.Email;
        existingLead.Phone = lead.Phone;
        existingLead.Company = lead.Company;
        existingLead.Status = lead.Status;
        existingLead.Source = lead.Source;
        existingLead.Priority = lead.Priority;

        if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            existingLead.AssignedUserId = lead.AssignedUserId;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Lead updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // CONVERT LEAD INTO CUSTOMER
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager,SalesExecutive")]
    public async Task<IActionResult> Convert(int id)
    {
        var lead = await _context.Leads.FindAsync(id);

        if (lead == null)
            return NotFound();

        if (User.IsInRole("SalesExecutive") &&
            lead.AssignedUserId != CurrentUserId())
            return Forbid();

        if (lead.Status == "Converted")
        {
            TempData["Error"] = "This lead has already been converted.";
            return RedirectToAction(nameof(Index));
        }

        bool customerExists = await _context.Customers
            .AnyAsync(c => c.Email == lead.Email);

        if (customerExists)
        {
            TempData["Error"] =
                "A customer with this email already exists. Lead was not converted.";

            return RedirectToAction(nameof(Index));
        }

        var customer = new Customer
        {
            Name = lead.Name,
            Email = lead.Email,
            Phone = lead.Phone,
            Company = lead.Company,
            AssignedUserId = lead.AssignedUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        lead.Status = "Converted";

        await _context.SaveChangesAsync();

        TempData["Success"] = "Lead converted into customer successfully.";
        return RedirectToAction(nameof(Index));
    }

    // DELETE LEAD - GET
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var lead = await _context.Leads
            .Include(l => l.AssignedUser)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lead == null)
            return NotFound();

        return View(lead);
    }

    // DELETE LEAD - POST
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var lead = await _context.Leads.FindAsync(id);

        if (lead == null)
            return NotFound();

        _context.Leads.Remove(lead);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Lead deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
