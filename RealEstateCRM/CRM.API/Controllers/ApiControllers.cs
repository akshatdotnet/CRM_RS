using CRM.API.DTOs;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeadsController : ControllerBase
{
    private readonly ILeadRepository _leads;
    private readonly ILeadActivityRepository _activities;
    private readonly ICustomerRepository _customers;

    public LeadsController(ILeadRepository leads, ILeadActivityRepository activities, ICustomerRepository customers)
    { _leads = leads; _activities = activities; _customers = customers; }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? stage, [FromQuery] string? q)
    {
        var leads = string.IsNullOrEmpty(q)
            ? string.IsNullOrEmpty(stage) ? await _leads.GetAllAsync() : await _leads.GetByStageAsync(stage)
            : await _leads.SearchAsync(q);
        return Ok(leads.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var l = await _leads.GetByIdAsync(id);
        return l == null ? NotFound() : Ok(ToDto(l));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateLeadDto dto)
    {
        var lead = new Lead { FullName = dto.FullName, Phone = dto.Phone, Email = dto.Email, LeadSource = dto.LeadSource, PropertyType = dto.PropertyType, LocationPreference = dto.LocationPreference, BudgetMin = dto.BudgetMin, BudgetMax = dto.BudgetMax, Stage = "New", AgentId = dto.AgentId, FollowUpDeadline = dto.FollowUpDeadline, CloseByDate = dto.CloseByDate, Notes = dto.Notes };
        await _leads.CreateAsync(lead);
        await _activities.CreateAsync(new LeadActivity { LeadId = lead.Id, ActivityType = "Created", Description = $"Lead created via API", ToStage = "New" });
        return CreatedAtAction(nameof(Get), new { id = lead.Id }, ToDto(lead));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateLeadDto dto)
    {
        var lead = await _leads.GetByIdAsync(id);
        if (lead == null) return NotFound();
        string old = lead.Stage;
        lead.FullName = dto.FullName; lead.Phone = dto.Phone; lead.Email = dto.Email; lead.LeadSource = dto.LeadSource;
        lead.PropertyType = dto.PropertyType; lead.LocationPreference = dto.LocationPreference;
        lead.BudgetMin = dto.BudgetMin; lead.BudgetMax = dto.BudgetMax; lead.Stage = dto.Stage;
        lead.AgentId = dto.AgentId; lead.FollowUpDeadline = dto.FollowUpDeadline; lead.CloseByDate = dto.CloseByDate; lead.Notes = dto.Notes;
        await _leads.UpdateAsync(lead);
        if (old != dto.Stage)
            await _activities.CreateAsync(new LeadActivity { LeadId = id, ActivityType = "Stage Change", Description = $"{old} → {dto.Stage}", FromStage = old, ToStage = dto.Stage });
        return Ok(ToDto(lead));
    }

    [HttpPatch("{id}/stage")]
    public async Task<IActionResult> UpdateStage(int id, UpdateStageDto dto)
    {
        var lead = await _leads.GetByIdAsync(id);
        if (lead == null) return NotFound();
        string old = lead.Stage; lead.Stage = dto.Stage;
        await _leads.UpdateAsync(lead);
        await _activities.CreateAsync(new LeadActivity { LeadId = id, ActivityType = "Stage Change", Description = $"{old} → {dto.Stage}", FromStage = old, ToStage = dto.Stage });
        if (dto.Stage == "Closed" && lead.Customer == null)
            await _customers.CreateAsync(new Customer { FullName = lead.FullName, Phone = lead.Phone, Email = lead.Email, PropertyPurchased = $"{lead.PropertyType}, {lead.LocationPreference}", DealValue = lead.BudgetMax, DealClosedDate = DateTime.UtcNow, Source = lead.LeadSource, LeadId = lead.Id, AgentId = lead.AgentId });
        return Ok(new { id, newStage = dto.Stage });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    { await _leads.DeleteAsync(id); return NoContent(); }

    [HttpGet("{id}/activities")]
    public async Task<IActionResult> GetActivities(int id)
    { return Ok(await _activities.GetByLeadIdAsync(id)); }

    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdue()
    { return Ok((await _leads.GetOverdueAsync()).Select(ToDto)); }

    private static LeadDto ToDto(Lead l) => new(l.Id, l.FullName, l.Phone, l.Email, l.LeadSource,
        l.PropertyType, l.LocationPreference, l.BudgetMin, l.BudgetMax, l.Stage, l.CreatedAt,
        l.FollowUpDeadline, l.CloseByDate, l.Notes, l.Agent?.FullName ?? "");
}

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _customers;
    public CustomersController(ICustomerRepository customers) => _customers = customers;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var all = await _customers.GetAllAsync();
        return Ok(all.Select(c => new CustomerDto(c.Id, c.FullName, c.Phone, c.PropertyPurchased, c.DealValue, c.DealClosedDate, c.Agent?.FullName ?? "", c.Source)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var c = await _customers.GetByIdAsync(id);
        return c == null ? NotFound() : Ok(new CustomerDto(c.Id, c.FullName, c.Phone, c.PropertyPurchased, c.DealValue, c.DealClosedDate, c.Agent?.FullName ?? "", c.Source));
    }
}

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ILeadRepository _leads;
    private readonly ICustomerRepository _customers;
    public DashboardController(ILeadRepository leads, ICustomerRepository customers) { _leads = leads; _customers = customers; }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var summary = await _leads.GetStageSummaryAsync();
        var revenue = await _customers.GetTotalRevenueAsync();
        int total = summary.Values.Sum();
        int closed = summary.GetValueOrDefault("Closed");
        return Ok(new DashboardSummaryDto(total, summary.GetValueOrDefault("Site Visit"), closed, revenue,
            total > 0 ? Math.Round((double)closed / total * 100, 1) : 0, summary));
    }
}

[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly IAgentRepository _agents;
    public AgentsController(IAgentRepository agents) => _agents = agents;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _agents.GetAllAsync());
}
