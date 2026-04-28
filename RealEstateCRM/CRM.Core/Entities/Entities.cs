namespace CRM.Core.Entities;

public class Lead
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
    public string Stage { get; set; } = "New";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime FollowUpDeadline { get; set; }
    public DateTime CloseByDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public int AgentId { get; set; }
    public Agent Agent { get; set; } = null!;
    public ICollection<LeadActivity> Activities { get; set; } = new List<LeadActivity>();
    public Customer? Customer { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PropertyPurchased { get; set; } = string.Empty;
    public decimal DealValue { get; set; }
    public DateTime DealClosedDate { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int LeadId { get; set; }
    public Lead Lead { get; set; } = null!;
    public int AgentId { get; set; }
    public Agent Agent { get; set; } = null!;
}

public class Agent
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}

public class LeadActivity
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public Lead Lead { get; set; } = null!;
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FromStage { get; set; } = string.Empty;
    public string ToStage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
