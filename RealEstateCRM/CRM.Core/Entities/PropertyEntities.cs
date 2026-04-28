namespace CRM.Core.Entities;

/// <summary>Property listing managed by admin, shared by agents to clients.</summary>
public class Property
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;   // 2BHK, Villa, etc.
    public string Status { get; set; } = "Available";           // Available, Sold, Hold
    public decimal Price { get; set; }
    public string PriceLabel { get; set; } = string.Empty;      // "₹1.8 Cr", "₹85L" etc.
    public string Location { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public double AreaSqFt { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int Floors { get; set; }
    public int YearBuilt { get; set; }
    public bool IsFurnished { get; set; }
    public bool HasParking { get; set; }
    public bool HasGym { get; set; }
    public bool HasPool { get; set; }
    public bool HasSecurity { get; set; }
    public string Amenities { get; set; } = string.Empty;       // comma-separated
    public string PublicSlug { get; set; } = string.Empty;      // unique URL slug
    public bool IsPublished { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int CreatedByAgentId { get; set; }
    public Agent CreatedBy { get; set; } = null!;

    public ICollection<PropertyPhoto> Photos { get; set; } = new List<PropertyPhoto>();
    public ICollection<PropertyEnquiry> Enquiries { get; set; } = new List<PropertyEnquiry>();
    public ICollection<DealConfirmation> DealConfirmations { get; set; } = new List<DealConfirmation>();
}

public class PropertyPhoto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;        // relative web path
    public string Caption { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>When a client submits interest from the public property page.</summary>
public class PropertyEnquiry
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public string ClientName { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "New";                 // New, Contacted, Converted
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string IPAddress { get; set; } = string.Empty;
}

/// <summary>Client confirms intent to buy — triggers deal closure workflow.</summary>
public class DealConfirmation
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public string ClientName { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string ClientPAN { get; set; } = string.Empty;       // KYC
    public string ClientAadhaar { get; set; } = string.Empty;
    public decimal OfferedPrice { get; set; }
    public string PaymentMode { get; set; } = string.Empty;     // Cash, Loan, Part-payment
    public string LoanBank { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string ConfirmationToken { get; set; } = string.Empty;  // secure unique token
    public string Status { get; set; } = "Pending";             // Pending, Reviewed, Approved, Rejected
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByAgentId { get; set; }
    public Agent? ReviewedBy { get; set; }
    public bool ClientConsent { get; set; }                     // GDPR/consent checkbox
}
