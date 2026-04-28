using System.ComponentModel.DataAnnotations;

namespace CRM.Web.Models;

public class DashboardViewModel
{
    public int TotalLeads { get; set; }
    public int SiteVisits { get; set; }
    public int ClosedDeals { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal TotalRevenue { get; set; }
    public Dictionary<string, int> StageSummary { get; set; } = new();
    public List<RecentLeadItem> RecentLeads { get; set; } = new();
    public List<ActivityItem> RecentActivities { get; set; } = new();
}

public class RecentLeadItem
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public DateTime FollowUpDeadline { get; set; }
}

public class ActivityItem
{
    public string Description { get; set; } = string.Empty;
    public string LeadName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ActivityType { get; set; } = string.Empty;
}

public class LeadFormViewModel
{
    public int Id { get; set; }
    [Required] public string FullName { get; set; } = string.Empty;
    [Required] public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    [Required] public string LeadSource { get; set; } = string.Empty;
    [Required] public string PropertyType { get; set; } = string.Empty;
    [Required] public string LocationPreference { get; set; } = string.Empty;
    [Range(0, double.MaxValue)] public decimal BudgetMin { get; set; }
    [Range(0, double.MaxValue)] public decimal BudgetMax { get; set; }
    public string Stage { get; set; } = "New";
    public int AgentId { get; set; }
    [Required] public DateTime FollowUpDeadline { get; set; } = DateTime.Today.AddDays(7);
    [Required] public DateTime CloseByDate { get; set; } = DateTime.Today.AddDays(30);
    public string Notes { get; set; } = string.Empty;
    public List<AgentSelectItem> Agents { get; set; } = new();
}

public class AgentSelectItem
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
}

public class LeadListViewModel
{
    public List<LeadRowItem> Leads { get; set; } = new();
    public string? StageFilter { get; set; }
    public string? SearchQuery { get; set; }
}

public class LeadRowItem
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public string LocationPreference { get; set; } = string.Empty;
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }
    public string LeadSource { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public DateTime FollowUpDeadline { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public bool IsOverdue => FollowUpDeadline < DateTime.Today && Stage != "Closed" && Stage != "Lost";
}

public class LeadDetailViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LeadSource { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public string LocationPreference { get; set; } = string.Empty;
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }
    public string Stage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime FollowUpDeadline { get; set; }
    public DateTime CloseByDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public List<ActivityItem> Activities { get; set; } = new();
    public int DaysInPipeline => (DateTime.Today - CreatedAt.Date).Days;
}

public class PipelineViewModel
{
    public Dictionary<string, List<PipelineCard>> Stages { get; set; } = new();
}

public class PipelineCard
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public string LocationPreference { get; set; } = string.Empty;
    public decimal BudgetMax { get; set; }
    public string LeadSource { get; set; } = string.Empty;
    public DateTime FollowUpDeadline { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string DeadlineStatus => FollowUpDeadline < DateTime.Today ? "overdue" : FollowUpDeadline <= DateTime.Today.AddDays(3) ? "warn" : "ok";
}

public class CustomerListViewModel
{
    public List<CustomerRowItem> Customers { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public int TotalCount { get; set; }
}

public class CustomerRowItem
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PropertyPurchased { get; set; } = string.Empty;
    public decimal DealValue { get; set; }
    public DateTime DealClosedDate { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}

public class AnalyticsViewModel
{
    public int TotalLeads { get; set; }
    public int ClosedDeals { get; set; }
    public int LostLeads { get; set; }
    public decimal TotalRevenue { get; set; }
    public double AvgDaysToClose { get; set; }
    public double ConversionRate { get; set; }
    public Dictionary<string, int> StageSummary { get; set; } = new();
    public Dictionary<string, double> StageConversion { get; set; } = new();
}
