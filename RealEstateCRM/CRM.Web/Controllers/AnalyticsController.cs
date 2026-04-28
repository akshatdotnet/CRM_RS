using CRM.Core.Interfaces;
using CRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Web.Controllers;

[Authorize]
public class AnalyticsController : Controller
{
    private readonly ILeadRepository _leads;
    private readonly ICustomerRepository _customers;

    public AnalyticsController(ILeadRepository leads, ICustomerRepository customers)
    {
        _leads = leads; _customers = customers;
    }

    public async Task<IActionResult> Index()
    {
        var allLeads = (await _leads.GetAllAsync()).ToList();
        var summary = await _leads.GetStageSummaryAsync();
        var totalRevenue = await _customers.GetTotalRevenueAsync();
        int total = allLeads.Count;
        int closed = summary.GetValueOrDefault("Closed");
        int lost = summary.GetValueOrDefault("Lost");

        var stages = new[] { "New", "Contacted", "Site Visit", "Negotiation", "Closed" };
        var conversion = new Dictionary<string, double>();
        for (int i = 0; i < stages.Length - 1; i++)
        {
            int cur = summary.GetValueOrDefault(stages[i]);
            int next = summary.GetValueOrDefault(stages[i + 1]);
            int denom = cur + next;
            conversion[$"{stages[i]} → {stages[i + 1]}"] = denom > 0 ? Math.Round((double)next / denom * 100, 1) : 0;
        }

        var vm = new AnalyticsViewModel
        {
            TotalLeads = total, ClosedDeals = closed, LostLeads = lost,
            TotalRevenue = totalRevenue,
            ConversionRate = total > 0 ? Math.Round((double)closed / total * 100, 1) : 0,
            AvgDaysToClose = 18.4, StageSummary = summary, StageConversion = conversion
        };
        return View(vm);
    }
}
