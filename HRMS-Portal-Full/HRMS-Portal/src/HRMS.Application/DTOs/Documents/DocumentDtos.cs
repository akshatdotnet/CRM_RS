using System.ComponentModel.DataAnnotations;
using HRMS.Domain.Enums;

namespace HRMS.Application.DTOs.Documents;

public class DocumentDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? GeneratedAt { get; init; }
    public DateTime? SentAt { get; init; }
    public string EmailStatus { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public class OfferLetterRequestDto
{
    [Required] public string DesignationOffered { get; set; } = string.Empty;
    [Required] public decimal OfferedCTC { get; set; }
    [Required] public DateTime JoiningDeadline { get; set; }
    public string? SpecialConditions { get; set; }
    public string? ReportingManager { get; set; }
    public string? WorkLocation { get; set; } = "Mumbai, India";
}

public class Form16RequestDto
{
    [Required, Range(2020, 2100)] public int AssessmentYear { get; set; } // e.g., 2024 means FY 2023-24
    [Required] public string TanNumber { get; set; } = string.Empty;
    public decimal TotalTaxDeducted { get; set; }
}
