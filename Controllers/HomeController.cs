
using System.Diagnostics;
using System.Text.Json;
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // CUSTOMER STATISTICS
        ViewBag.TotalCustomers = await _context.Customers.CountAsync();

        // LEAD STATISTICS
        var leads = await _context.Leads
            .AsNoTracking()
            .ToListAsync();

        ViewBag.TotalLeads = leads.Count;
        ViewBag.OpenLeads = leads.Count(l =>
            l.Status != "Converted" && l.Status != "Lost");

        ViewBag.ConvertedLeads = leads.Count(l =>
            l.Status == "Converted");

        ViewBag.LostLeads = leads.Count(l =>
            l.Status == "Lost");

        // OPPORTUNITY STATISTICS
        var opportunities = await _context.Opportunities
            .AsNoTracking()
            .ToListAsync();

        ViewBag.TotalOpportunities = opportunities.Count;

        ViewBag.WonOpportunities = opportunities.Count(o =>
            o.Stage == "Won");

        ViewBag.LostOpportunities = opportunities.Count(o =>
            o.Stage == "Lost");

        var openOpportunities = opportunities
            .Where(o => o.Stage != "Won" && o.Stage != "Lost")
            .ToList();

        ViewBag.OpenPipeline = openOpportunities.Sum(o => o.Amount);

        ViewBag.WeightedPipeline = openOpportunities.Sum(o =>
            o.Amount * o.Probability / 100m);

        // LEAD STATUS CHART
        var leadStatuses = leads
            .GroupBy(l => l.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToList();

        ViewBag.LeadChartLabels = JsonSerializer.Serialize(
            leadStatuses.Select(x => x.Status));

        ViewBag.LeadChartValues = JsonSerializer.Serialize(
            leadStatuses.Select(x => x.Count));

        // OPPORTUNITY PIPELINE CHART
        var pipelineStages = openOpportunities
            .GroupBy(o => o.Stage)
            .Select(g => new
            {
                Stage = g.Key,
                Amount = g.Sum(o => o.Amount)
            })
            .ToList();

        ViewBag.PipelineChartLabels = JsonSerializer.Serialize(
            pipelineStages.Select(x => x.Stage));

        ViewBag.PipelineChartValues = JsonSerializer.Serialize(
            pipelineStages.Select(x => x.Amount));

        // MONTHLY WON SALES CHART - LAST 6 MONTHS
        var today = DateTime.Today;
        var firstMonth = new DateTime(today.Year, today.Month, 1)
            .AddMonths(-5);

        var monthlyLabels = Enumerable.Range(0, 6)
            .Select(i => firstMonth.AddMonths(i).ToString("MMM yyyy"))
            .ToList();

        var monthlySales = Enumerable.Range(0, 6)
            .Select(i =>
            {
                var month = firstMonth.AddMonths(i);

                return opportunities
                    .Where(o => o.Stage == "Won" &&
                                o.CreatedAt.Year == month.Year &&
                                o.CreatedAt.Month == month.Month)
                    .Sum(o => o.Amount);
            })
            .ToList();

        ViewBag.MonthlySalesLabels =
            JsonSerializer.Serialize(monthlyLabels);

        ViewBag.MonthlySalesValues =
            JsonSerializer.Serialize(monthlySales);

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
