using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers;

[Authorize]
public class LeadsController : Controller
{
    private readonly ApplicationDbContext _context;

    public LeadsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Leads.OrderByDescending(l => l.Id).ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Lead lead)
    {
        if (ModelState.IsValid)
        {
            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(lead);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var lead = await _context.Leads.FindAsync(id);
        if (lead == null) return NotFound();

        return View(lead);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Lead lead)
    {
        if (id != lead.Id) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(lead);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(lead);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var lead = await _context.Leads.FindAsync(id);
        if (lead == null) return NotFound();

        return View(lead);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var lead = await _context.Leads.FindAsync(id);

        if (lead != null)
        {
            _context.Leads.Remove(lead);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}