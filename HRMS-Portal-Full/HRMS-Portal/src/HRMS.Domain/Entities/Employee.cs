using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class Employee : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty; // e.g., EMP-0001
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Department Department { get; set; }
    public string Designation { get; set; } = string.Empty;
    public DateTime JoiningDate { get; set; }
    public DateTime? ResignationDate { get; set; }
    public DateTime? LastWorkingDate { get; set; }
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? PanNumber { get; set; }     // India compliance
    public string? AadhaarNumber { get; set; } // India compliance (store masked)
    public string? Address { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // Navigation
    public SalaryStructure? SalaryStructure { get; set; }
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<SalarySlip> SalarySlips { get; set; } = new List<SalarySlip>();

    // Computed
    public string FullName => $"{FirstName} {LastName}";

    public int GetTenureInMonths()
    {
        var endDate = LastWorkingDate ?? DateTime.UtcNow;
        return ((endDate.Year - JoiningDate.Year) * 12) + endDate.Month - JoiningDate.Month;
    }

    public int GetTenureInYears() => GetTenureInMonths() / 12;
}
