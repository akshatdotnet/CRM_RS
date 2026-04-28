namespace CRM.API.DTOs;

public record LeadDto(int Id, string FullName, string Phone, string Email, string LeadSource,
    string PropertyType, string LocationPreference, decimal BudgetMin, decimal BudgetMax,
    string Stage, DateTime CreatedAt, DateTime FollowUpDeadline, DateTime CloseByDate,
    string Notes, string AgentName);

public record CreateLeadDto(string FullName, string Phone, string Email, string LeadSource,
    string PropertyType, string LocationPreference, decimal BudgetMin, decimal BudgetMax,
    int AgentId, DateTime FollowUpDeadline, DateTime CloseByDate, string Notes = "");

public record UpdateLeadDto(string FullName, string Phone, string Email, string LeadSource,
    string PropertyType, string LocationPreference, decimal BudgetMin, decimal BudgetMax,
    string Stage, int AgentId, DateTime FollowUpDeadline, DateTime CloseByDate, string Notes);

public record UpdateStageDto(string Stage);

public record CustomerDto(int Id, string FullName, string Phone, string PropertyPurchased,
    decimal DealValue, DateTime DealClosedDate, string AgentName, string Source);

public record DashboardSummaryDto(int TotalLeads, int SiteVisits, int ClosedDeals,
    decimal TotalRevenue, double ConversionRate, Dictionary<string, int> StageSummary);
