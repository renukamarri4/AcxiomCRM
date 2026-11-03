
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AcxiomCRM.Controllers;

[Authorize(Roles = "Admin,Manager,SalesExecutive")]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CustomersController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task LoadUsers(string? selectedUserId = null)
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        ViewBag.Users = new SelectList(
            users,
            "Id",
            "Email",
            selectedUserId
        );
    }

    private string? CurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private async Task<bool> IsValidAssignedUser(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        return await _userManager.FindByIdAsync(userId) != null;
    }

    // CUSTOMER LIST AND SEARCH
    public async Task<IActionResult> Index(string? search)
    {
        var customers = _context.Customers
            .Include(c => c.AssignedUser)
            .AsQueryable();

        // SalesExecutive sees only assigned customers
        if (User.IsInRole("SalesExecutive"))
        {
            var userId = CurrentUserId();
            customers = customers.Where(c => c.AssignedUserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            customers = customers.Where(c =>
                c.Name.Contains(search) ||
                c.Email.Contains(search) ||
                c.Company.Contains(search));
        }

        ViewBag.Search = search;

        return View(await customers
            .OrderByDescending(c => c.Id)
            .ToListAsync());
    }

    // CREATE CUSTOMER - GET
    public async Task<IActionResult> Create()
    {
        if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            await LoadUsers();

        return View();
    }

    // CREATE CUSTOMER - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (User.IsInRole("SalesExecutive"))
        {
            customer.AssignedUserId = CurrentUserId();
        }
        else
        {
            if (!await IsValidAssignedUser(customer.AssignedUserId))
            {
                ModelState.AddModelError(
                    "AssignedUserId",
                    "Please select a valid user."
                );
            }
        }

        // Prevent duplicate email addresses
        bool emailExists = await _context.Customers.AnyAsync(c =>
            c.Email == customer.Email);

        if (emailExists)
        {
            ModelState.AddModelError(
                "Email",
                "A customer with this email already exists."
            );
        }

        // Prevent duplicate phone numbers
        bool phoneExists = await _context.Customers.AnyAsync(c =>
            c.Phone == customer.Phone);

        if (phoneExists)
        {
            ModelState.AddModelError(
                "Phone",
                "A customer with this phone number already exists."
            );
        }

        if (!ModelState.IsValid)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                await LoadUsers(customer.AssignedUserId);

            return View(customer);
        }

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Customer created successfully.";

        return RedirectToAction(nameof(Index));
    }

    // EDIT CUSTOMER - GET
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound();

        if (User.IsInRole("SalesExecutive") &&
            customer.AssignedUserId != CurrentUserId())
        {
            return Forbid();
        }

        if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            await LoadUsers(customer.AssignedUserId);

        return View(customer);
    }

    // EDIT CUSTOMER - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Customer customer)
    {
        if (id != customer.Id)
            return NotFound();

        var existingCustomer = await _context.Customers.FindAsync(id);

        if (existingCustomer == null)
            return NotFound();

        if (User.IsInRole("SalesExecutive"))
        {
            if (existingCustomer.AssignedUserId != CurrentUserId())
                return Forbid();
        }
        else
        {
            if (!await IsValidAssignedUser(customer.AssignedUserId))
            {
                ModelState.AddModelError(
                    "AssignedUserId",
                    "Please select a valid user."
                );
            }
        }

        // Check duplicate email excluding this customer
        bool emailExists = await _context.Customers.AnyAsync(c =>
            c.Email == customer.Email && c.Id != id);

        if (emailExists)
        {
            ModelState.AddModelError(
                "Email",
                "Another customer already uses this email."
            );
        }

        // Check duplicate phone excluding this customer
        bool phoneExists = await _context.Customers.AnyAsync(c =>
            c.Phone == customer.Phone && c.Id != id);

        if (phoneExists)
        {
            ModelState.AddModelError(
                "Phone",
                "Another customer already uses this phone number."
            );
        }

        if (!ModelState.IsValid)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                await LoadUsers(customer.AssignedUserId);

            return View(customer);
        }

        existingCustomer.Name = customer.Name;
        existingCustomer.Email = customer.Email;
        existingCustomer.Phone = customer.Phone;
        existingCustomer.Company = customer.Company;
        existingCustomer.Address = customer.Address;

        if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            existingCustomer.AssignedUserId = customer.AssignedUserId;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Customer updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // DELETE CUSTOMER - GET
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var customer = await _context.Customers
            .Include(c => c.AssignedUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
            return NotFound();

        return View(customer);
    }

    // DELETE CUSTOMER - POST
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound();

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Customer deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}
