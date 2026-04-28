using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CRM.Web.Models;

public class PropertyFormViewModel
{
    public int Id { get; set; }

    [Required] public string Title { get; set; } = string.Empty;
    [Required] public string PropertyType { get; set; } = string.Empty;
    [Required] public string Location { get; set; } = string.Empty;
    [Required] public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    [Required, Range(1, double.MaxValue)] public decimal Price { get; set; }
    public string PriceLabel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double AreaSqFt { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int Floors { get; set; }
    public int YearBuilt { get; set; } = DateTime.Now.Year;
    public bool IsFurnished { get; set; }
    public bool HasParking { get; set; }
    public bool HasGym { get; set; }
    public bool HasPool { get; set; }
    public bool HasSecurity { get; set; }
    public string Amenities { get; set; } = string.Empty;
    public string Status { get; set; } = "Available";
    public bool IsPublished { get; set; } = true;
    public int CreatedByAgentId { get; set; }

    // Photo uploads
    public List<IFormFile> Photos { get; set; } = new();
    public List<AgentSelectItem> Agents { get; set; } = new();
}

public class PropertyListViewModel
{
    public List<PropertyCardItem> Properties { get; set; } = new();
    public string? StatusFilter { get; set; }
    public string? SearchQuery { get; set; }
    public int TotalAvailable { get; set; }
    public int TotalSold { get; set; }
    public int TotalOnHold { get; set; }
    public int PendingEnquiries { get; set; }
    public int PendingConfirmations { get; set; }
}

public class PropertyCardItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PriceLabel { get; set; } = string.Empty;
    public double AreaSqFt { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PublicSlug { get; set; } = string.Empty;
    public string? PrimaryPhotoPath { get; set; }
    public int EnquiryCount { get; set; }
}

public class PropertyDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PriceLabel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
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
    public List<string> Amenities { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public string PublicSlug { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public string CreatedByAgent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<PhotoItem> Photos { get; set; } = new();
    public List<EnquiryItem> Enquiries { get; set; } = new();
    public List<ConfirmationItem> Confirmations { get; set; } = new();
    public string ShareableUrl { get; set; } = string.Empty;
}

public class PhotoItem
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class EnquiryItem
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ConfirmationItem
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public decimal OfferedPrice { get; set; }
    public string PaymentMode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string ConfirmationToken { get; set; } = string.Empty;
}

// ── Public-facing (client) view models ──────────────────────────────
public class PublicPropertyViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PriceLabel { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
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
    public List<string> Amenities { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string AgentPhone { get; set; } = string.Empty;
    public List<PhotoItem> Photos { get; set; } = new();
    public string Slug { get; set; } = string.Empty;
}

public class ClientEnquiryFormViewModel
{
    public int PropertyId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    [Required] public string ClientName { get; set; } = string.Empty;
    [Required, Phone] public string ClientPhone { get; set; } = string.Empty;
    [EmailAddress] public string ClientEmail { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ClientDealConfirmationViewModel
{
    public int PropertyId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public string PropertyLocation { get; set; } = string.Empty;
    public string PriceLabel { get; set; } = string.Empty;
    public string? PrimaryPhoto { get; set; }

    [Required] public string ClientName { get; set; } = string.Empty;
    [Required, Phone] public string ClientPhone { get; set; } = string.Empty;
    [Required, EmailAddress] public string ClientEmail { get; set; } = string.Empty;
    public string ClientPAN { get; set; } = string.Empty;
    public string ClientAadhaar { get; set; } = string.Empty;
    [Required, Range(1, double.MaxValue, ErrorMessage = "Please enter your offered price")]
    public decimal OfferedPrice { get; set; }
    [Required] public string PaymentMode { get; set; } = string.Empty;
    public string LoanBank { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must consent to proceed")]
    public bool ClientConsent { get; set; }
}

public class ConfirmationSuccessViewModel
{
    public string ClientName { get; set; } = string.Empty;
    public string PropertyTitle { get; set; } = string.Empty;
    public string ConfirmationToken { get; set; } = string.Empty;
    public string AgentPhone { get; set; } = string.Empty;
}
