
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers;

[Authorize]
public class FollowUpsController : Controller
{
    private readonly ApplicationDbContext _context;

    public FollowUpsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // All three roles can view follow-ups
    [Authorize(Roles = "Admin,Manager,SalesExecutive")]
    public async Task<IActionResult> Index()
    {
        return View(await _context.FollowUps
            .OrderByDescending(f => f.Id)
            .ToListAsync());
    }

    // All three roles can create follow-ups
    [Authorize(Roles = "Admin,Manager,SalesExecutive")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager,SalesExecutive")]
    public async Task<IActionResult> Create(FollowUp followUp)
    {
        if (ModelState.IsValid)
        {
            _context.FollowUps.Add(followUp);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(followUp);
    }

    // All three roles can edit follow-ups
    [Authorize(Roles = "Admin,Manager,SalesExecutive")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var followUp = await _context.FollowUps.FindAsync(id);

        if (followUp == null)
            return NotFound();

        return View(followUp);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager,SalesExecutive")]
    public async Task<IActionResult> Edit(int id, FollowUp followUp)
    {
        if (id != followUp.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(followUp);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(followUp);
    }

    // Only Admin and Manager can open Delete page
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var followUp = await _context.FollowUps
            .FirstOrDefaultAsync(f => f.Id == id);

        if (followUp == null)
            return NotFound();

        return View(followUp);
    }

    // Only Admin and Manager can delete follow-ups
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var followUp = await _context.FollowUps.FindAsync(id);

        if (followUp != null)
        {
            _context.FollowUps.Remove(followUp);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
