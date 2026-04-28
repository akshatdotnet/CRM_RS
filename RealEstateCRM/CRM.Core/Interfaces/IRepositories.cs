using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

public interface ILeadRepository
{
    Task<IEnumerable<Lead>> GetAllAsync();
    Task<Lead?> GetByIdAsync(int id);
    Task<IEnumerable<Lead>> GetByStageAsync(string stage);
    Task<IEnumerable<Lead>> GetOverdueAsync();
    Task<IEnumerable<Lead>> GetByAgentAsync(int agentId);
    Task<Lead> CreateAsync(Lead lead);
    Task<Lead> UpdateAsync(Lead lead);
    Task DeleteAsync(int id);
    Task<Dictionary<string, int>> GetStageSummaryAsync();
    Task<IEnumerable<Lead>> SearchAsync(string query);
}

public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer> CreateAsync(Customer customer);
    Task<Customer> UpdateAsync(Customer customer);
    Task<decimal> GetTotalRevenueAsync();
    Task<int> GetClosedCountThisMonthAsync();
}

public interface IAgentRepository
{
    Task<IEnumerable<Agent>> GetAllAsync();
    Task<Agent?> GetByIdAsync(int id);
    Task<Agent> CreateAsync(Agent agent);
}

public interface ILeadActivityRepository
{
    Task<IEnumerable<LeadActivity>> GetByLeadIdAsync(int leadId);
    Task<IEnumerable<LeadActivity>> GetRecentAsync(int count = 10);
    Task<LeadActivity> CreateAsync(LeadActivity activity);
}
