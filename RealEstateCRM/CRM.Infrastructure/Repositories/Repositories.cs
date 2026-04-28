using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly CrmDbContext _db;
    public LeadRepository(CrmDbContext db) => _db = db;

    public async Task<IEnumerable<Lead>> GetAllAsync() =>
        await _db.Leads.Include(l => l.Agent).Include(l => l.Activities).Where(l => l.IsActive).OrderByDescending(l => l.CreatedAt).ToListAsync();

    public async Task<Lead?> GetByIdAsync(int id) =>
        await _db.Leads.Include(l => l.Agent).Include(l => l.Activities).Include(l => l.Customer).FirstOrDefaultAsync(l => l.Id == id);

    public async Task<IEnumerable<Lead>> GetByStageAsync(string stage) =>
        await _db.Leads.Include(l => l.Agent).Where(l => l.Stage == stage && l.IsActive).OrderBy(l => l.FollowUpDeadline).ToListAsync();

    public async Task<IEnumerable<Lead>> GetOverdueAsync() =>
        await _db.Leads.Include(l => l.Agent).Where(l => l.FollowUpDeadline < DateTime.UtcNow && l.Stage != "Closed" && l.Stage != "Lost" && l.IsActive).ToListAsync();

    public async Task<IEnumerable<Lead>> GetByAgentAsync(int agentId) =>
        await _db.Leads.Include(l => l.Agent).Where(l => l.AgentId == agentId && l.IsActive).OrderByDescending(l => l.CreatedAt).ToListAsync();

    public async Task<Lead> CreateAsync(Lead lead)
    {
        _db.Leads.Add(lead);
        await _db.SaveChangesAsync();
        return lead;
    }

    public async Task<Lead> UpdateAsync(Lead lead)
    {
        _db.Leads.Update(lead);
        await _db.SaveChangesAsync();
        return lead;
    }

    public async Task DeleteAsync(int id)
    {
        var lead = await _db.Leads.FindAsync(id);
        if (lead != null) { lead.IsActive = false; await _db.SaveChangesAsync(); }
    }

    public async Task<Dictionary<string, int>> GetStageSummaryAsync()
    {
        var stages = new[] { "New", "Contacted", "Site Visit", "Negotiation", "Closed", "Lost" };
        var counts = await _db.Leads.Where(l => l.IsActive).GroupBy(l => l.Stage).Select(g => new { Stage = g.Key, Count = g.Count() }).ToListAsync();
        return stages.ToDictionary(s => s, s => counts.FirstOrDefault(c => c.Stage == s)?.Count ?? 0);
    }

    public async Task<IEnumerable<Lead>> SearchAsync(string query) =>
        await _db.Leads.Include(l => l.Agent).Where(l => l.IsActive && (l.FullName.Contains(query) || l.Phone.Contains(query) || l.Email.Contains(query) || l.LocationPreference.Contains(query))).ToListAsync();
}

public class CustomerRepository : ICustomerRepository
{
    private readonly CrmDbContext _db;
    public CustomerRepository(CrmDbContext db) => _db = db;

    public async Task<IEnumerable<Customer>> GetAllAsync() =>
        await _db.Customers.Include(c => c.Agent).Include(c => c.Lead).OrderByDescending(c => c.DealClosedDate).ToListAsync();

    public async Task<Customer?> GetByIdAsync(int id) =>
        await _db.Customers.Include(c => c.Agent).Include(c => c.Lead).ThenInclude(l => l.Activities).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Customer> CreateAsync(Customer customer)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateAsync(Customer customer)
    {
        _db.Customers.Update(customer);
        await _db.SaveChangesAsync();
        return customer;
    }

    public async Task<decimal> GetTotalRevenueAsync() =>
        await _db.Customers.SumAsync(c => c.DealValue);

    public async Task<int> GetClosedCountThisMonthAsync()
    {
        var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        return await _db.Customers.CountAsync(c => c.DealClosedDate >= start);
    }
}

public class AgentRepository : IAgentRepository
{
    private readonly CrmDbContext _db;
    public AgentRepository(CrmDbContext db) => _db = db;

    public async Task<IEnumerable<Agent>> GetAllAsync() =>
        await _db.Agents.Where(a => a.IsActive).OrderBy(a => a.FullName).ToListAsync();

    public async Task<Agent?> GetByIdAsync(int id) =>
        await _db.Agents.Include(a => a.Leads).Include(a => a.Customers).FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Agent> CreateAsync(Agent agent)
    {
        _db.Agents.Add(agent);
        await _db.SaveChangesAsync();
        return agent;
    }
}

public class LeadActivityRepository : ILeadActivityRepository
{
    private readonly CrmDbContext _db;
    public LeadActivityRepository(CrmDbContext db) => _db = db;

    public async Task<IEnumerable<LeadActivity>> GetByLeadIdAsync(int leadId) =>
        await _db.LeadActivities.Where(a => a.LeadId == leadId).OrderByDescending(a => a.CreatedAt).ToListAsync();

    public async Task<IEnumerable<LeadActivity>> GetRecentAsync(int count = 10) =>
        await _db.LeadActivities.Include(a => a.Lead).OrderByDescending(a => a.CreatedAt).Take(count).ToListAsync();

    public async Task<LeadActivity> CreateAsync(LeadActivity activity)
    {
        _db.LeadActivities.Add(activity);
        await _db.SaveChangesAsync();
        return activity;
    }
}
