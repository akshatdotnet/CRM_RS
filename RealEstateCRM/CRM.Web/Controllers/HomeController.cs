using CRM.Core.Interfaces;
using CRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILeadRepository _leads;
    private readonly ICustomerRepository _customers;
    private readonly ILeadActivityRepository _activities;

    public HomeController(ILeadRepository leads, ICustomerRepository customers, ILeadActivityRepository activities)
    {
        _leads = leads; _customers = customers; _activities = activities;
    }

    public async Task<IActionResult> Index()
    {
        var allLeads = (await _leads.GetAllAsync()).ToList();
        var summary = await _leads.GetStageSummaryAsync();
        var recentActivities = (await _activities.GetRecentAsync(8)).ToList();
        var totalRevenue = await _customers.GetTotalRevenueAsync();

        int total = allLeads.Count;
        int closed = summary.GetValueOrDefault("Closed");

        var vm = new DashboardViewModel
        {
            TotalLeads = total,
            SiteVisits = summary.GetValueOrDefault("Site Visit"),
            ClosedDeals = closed,
            ConversionRate = total > 0 ? Math.Round((decimal)closed / total * 100, 1) : 0,
            TotalRevenue = totalRevenue,
            StageSummary = summary,
            RecentLeads = allLeads.Take(5).Select(l => new RecentLeadItem
            {
                Id = l.Id, FullName = l.FullName, PropertyType = l.PropertyType,
                Location = l.LocationPreference, Stage = l.Stage, FollowUpDeadline = l.FollowUpDeadline
            }).ToList(),
            RecentActivities = recentActivities.Select(a => new ActivityItem
            {
                Description = a.Description, LeadName = a.Lead?.FullName ?? "",
                CreatedAt = a.CreatedAt, ActivityType = a.ActivityType
            }).ToList()
        };
        return View(vm);
    }

    public IActionResult Error() => View();
}
