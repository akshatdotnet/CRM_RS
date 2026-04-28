using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Web.Controllers;

[Authorize]
public class LeadController : Controller
{
    private readonly ILeadRepository _leads;
    private readonly IAgentRepository _agents;
    private readonly ILeadActivityRepository _activities;
    private readonly ICustomerRepository _customers;

    public LeadController(ILeadRepository leads, IAgentRepository agents, ILeadActivityRepository activities, ICustomerRepository customers)
    {
        _leads = leads; _agents = agents; _activities = activities; _customers = customers;
    }

    public async Task<IActionResult> Index(string? stage, string? q)
    {
        IEnumerable<Lead> leads;
        if (!string.IsNullOrEmpty(q))
            leads = await _leads.SearchAsync(q);
        else if (!string.IsNullOrEmpty(stage))
            leads = await _leads.GetByStageAsync(stage);
        else
            leads = await _leads.GetAllAsync();

        var vm = new LeadListViewModel
        {
            StageFilter = stage,
            SearchQuery = q,
            Leads = leads.Select(l => new LeadRowItem
            {
                Id = l.Id, FullName = l.FullName, Phone = l.Phone,
                PropertyType = l.PropertyType, LocationPreference = l.LocationPreference,
                BudgetMin = l.BudgetMin, BudgetMax = l.BudgetMax,
                LeadSource = l.LeadSource, Stage = l.Stage,
                FollowUpDeadline = l.FollowUpDeadline, AgentName = l.Agent?.FullName ?? ""
            }).ToList()
        };
        return View(vm);
    }

    public async Task<IActionResult> Pipeline()
    {
        var allLeads = await _leads.GetAllAsync();
        var stages = new[] { "New", "Contacted", "Site Visit", "Negotiation", "Closed" };
        var vm = new PipelineViewModel
        {
            Stages = stages.ToDictionary(s => s, s => allLeads.Where(l => l.Stage == s).Select(l => new PipelineCard
            {
                Id = l.Id, FullName = l.FullName, PropertyType = l.PropertyType,
                LocationPreference = l.LocationPreference, BudgetMax = l.BudgetMax,
                LeadSource = l.LeadSource, FollowUpDeadline = l.FollowUpDeadline, AgentName = l.Agent?.FullName ?? ""
            }).ToList())
        };
        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var lead = await _leads.GetByIdAsync(id);
        if (lead == null) return NotFound();
        var acts = (await _activities.GetByLeadIdAsync(id)).ToList();
        var vm = new LeadDetailViewModel
        {
            Id = lead.Id, FullName = lead.FullName, Phone = lead.Phone, Email = lead.Email,
            LeadSource = lead.LeadSource, PropertyType = lead.PropertyType,
            LocationPreference = lead.LocationPreference, BudgetMin = lead.BudgetMin,
            BudgetMax = lead.BudgetMax, Stage = lead.Stage, CreatedAt = lead.CreatedAt,
            FollowUpDeadline = lead.FollowUpDeadline, CloseByDate = lead.CloseByDate,
            Notes = lead.Notes, AgentName = lead.Agent?.FullName ?? "",
            Activities = acts.Select(a => new ActivityItem
            {
                Description = a.Description, ActivityType = a.ActivityType, CreatedAt = a.CreatedAt
            }).ToList()
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var agents = await _agents.GetAllAsync();
        return View(new LeadFormViewModel
        {
            Agents = agents.Select(a => new AgentSelectItem { Id = a.Id, FullName = a.FullName }).ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeadFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var agents = await _agents.GetAllAsync();
            vm.Agents = agents.Select(a => new AgentSelectItem { Id = a.Id, FullName = a.FullName }).ToList();
            return View(vm);
        }
        var lead = new Lead
        {
            FullName = vm.FullName, Phone = vm.Phone, Email = vm.Email,
            LeadSource = vm.LeadSource, PropertyType = vm.PropertyType,
            LocationPreference = vm.LocationPreference, BudgetMin = vm.BudgetMin,
            BudgetMax = vm.BudgetMax, Stage = "New", AgentId = vm.AgentId,
            FollowUpDeadline = vm.FollowUpDeadline, CloseByDate = vm.CloseByDate, Notes = vm.Notes
        };
        await _leads.CreateAsync(lead);
        await _activities.CreateAsync(new LeadActivity { LeadId = lead.Id, ActivityType = "Created", Description = $"Lead created from {lead.LeadSource}", ToStage = "New" });
        TempData["Success"] = "Lead created successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var lead = await _leads.GetByIdAsync(id);
        if (lead == null) return NotFound();
        var agents = await _agents.GetAllAsync();
        return View(new LeadFormViewModel
        {
            Id = lead.Id, FullName = lead.FullName, Phone = lead.Phone, Email = lead.Email,
            LeadSource = lead.LeadSource, PropertyType = lead.PropertyType,
            LocationPreference = lead.LocationPreference, BudgetMin = lead.BudgetMin,
            BudgetMax = lead.BudgetMax, Stage = lead.Stage, AgentId = lead.AgentId,
            FollowUpDeadline = lead.FollowUpDeadline, CloseByDate = lead.CloseByDate, Notes = lead.Notes,
            Agents = agents.Select(a => new AgentSelectItem { Id = a.Id, FullName = a.FullName }).ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LeadFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var agents = await _agents.GetAllAsync();
            vm.Agents = agents.Select(a => new AgentSelectItem { Id = a.Id, FullName = a.FullName }).ToList();
            return View(vm);
        }
        var lead = await _leads.GetByIdAsync(id);
        if (lead == null) return NotFound();
        string oldStage = lead.Stage;
        lead.FullName = vm.FullName; lead.Phone = vm.Phone; lead.Email = vm.Email;
        lead.LeadSource = vm.LeadSource; lead.PropertyType = vm.PropertyType;
        lead.LocationPreference = vm.LocationPreference; lead.BudgetMin = vm.BudgetMin;
        lead.BudgetMax = vm.BudgetMax; lead.Stage = vm.Stage; lead.AgentId = vm.AgentId;
        lead.FollowUpDeadline = vm.FollowUpDeadline; lead.CloseByDate = vm.CloseByDate; lead.Notes = vm.Notes;
        await _leads.UpdateAsync(lead);
        if (oldStage != vm.Stage)
            await _activities.CreateAsync(new LeadActivity { LeadId = id, ActivityType = "Stage Change", Description = $"Stage updated from {oldStage} to {vm.Stage}", FromStage = oldStage, ToStage = vm.Stage });
        if (vm.Stage == "Closed" && lead.Customer == null)
        {
            await _customers.CreateAsync(new Customer
            {
                FullName = lead.FullName, Phone = lead.Phone, Email = lead.Email,
                PropertyPurchased = $"{lead.PropertyType}, {lead.LocationPreference}",
                DealValue = lead.BudgetMax, DealClosedDate = DateTime.UtcNow,
                Source = lead.LeadSource, LeadId = lead.Id, AgentId = lead.AgentId
            });
        }
        TempData["Success"] = "Lead updated successfully!";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStage(int id, string stage)
    {
        var lead = await _leads.GetByIdAsync(id);
        if (lead == null) return Json(new { success = false });
        string old = lead.Stage;
        lead.Stage = stage;
        await _leads.UpdateAsync(lead);
        await _activities.CreateAsync(new LeadActivity { LeadId = id, ActivityType = "Stage Change", Description = $"Stage moved from {old} to {stage}", FromStage = old, ToStage = stage });
        if (stage == "Closed" && lead.Customer == null)
            await _customers.CreateAsync(new Customer { FullName = lead.FullName, Phone = lead.Phone, Email = lead.Email, PropertyPurchased = $"{lead.PropertyType}, {lead.LocationPreference}", DealValue = lead.BudgetMax, DealClosedDate = DateTime.UtcNow, Source = lead.LeadSource, LeadId = lead.Id, AgentId = lead.AgentId });
        return Json(new { success = true, newStage = stage });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNote(int id, string note)
    {
        await _activities.CreateAsync(new LeadActivity { LeadId = id, ActivityType = "Note", Description = note });
        TempData["Success"] = "Note added.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _leads.DeleteAsync(id);
        TempData["Success"] = "Lead deleted.";
        return RedirectToAction(nameof(Index));
    }
}
