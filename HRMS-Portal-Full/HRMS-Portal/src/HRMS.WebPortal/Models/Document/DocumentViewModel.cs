using System.ComponentModel.DataAnnotations;

namespace HRMS.WebPortal.Models.Document;

public class DocumentListViewModel
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";
    public List<DocumentRowViewModel> Documents { get; set; } = new();
}

public class DocumentRowViewModel
{
    public Guid Id { get; set; }
    public string EmployeeName { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public string EmailStatus { get; set; } = "";
    public DateTime? GeneratedAt { get; set; }
}

public class GenerateOfferLetterViewModel
{
    [Required] public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";
    [Required] public string DesignationOffered { get; set; } = "";
    [Required, Range(1, 100_000_000)] public decimal OfferedCTC { get; set; }
    [Required] public DateTime JoiningDeadline { get; set; } = DateTime.Today.AddDays(7);
    public string? ReportingManager { get; set; }
    public string? WorkLocation { get; set; } = "Mumbai, India";
    public string? SpecialConditions { get; set; }
    public string? ErrorMessage { get; set; }
}
