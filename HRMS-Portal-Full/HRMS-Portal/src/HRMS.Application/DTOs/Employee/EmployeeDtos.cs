using System.ComponentModel.DataAnnotations;
using HRMS.Domain.Enums;

namespace HRMS.Application.DTOs.Employee;

public class EmployeeDto
{
    public Guid Id { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Designation { get; init; } = string.Empty;
    public DateTime JoiningDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Gender { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public string? PanNumber { get; init; }
    public string? Address { get; init; }
    public SalaryStructureDto? SalaryStructure { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
}

public class EmployeeListDto
{
    public Guid Id { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Designation { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime JoiningDate { get; init; }
    public decimal? GrossMonthly { get; init; }
}

public class SalaryStructureDto
{
    public decimal GrossMonthly { get; init; }
    public decimal Basic { get; init; }
    public decimal HRA { get; init; }
    public decimal SpecialAllowance { get; init; }
    public decimal EmployeePF { get; init; }
    public decimal ProfessionalTax { get; init; }
    public decimal NetMonthly { get; init; }
    public decimal AnnualCTC { get; init; }
}

public class CreateEmployeeDto
{
    [Required, MaxLength(100)] public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, Phone] public string Phone { get; set; } = string.Empty;
    [Required] public Department Department { get; set; }
    [Required, MaxLength(150)] public string Designation { get; set; } = string.Empty;
    [Required] public DateTime JoiningDate { get; set; }
    [Required] public Gender Gender { get; set; }
    [Required] public DateTime DateOfBirth { get; set; }
    public string? PanNumber { get; set; }
    public string? Address { get; set; }
    [Required] public CreateSalaryDto Salary { get; set; } = null!;
    [Required] public UserRole RoleToAssign { get; set; } = UserRole.Employee;
}

public class CreateSalaryDto
{
    [Required, Range(1, 10000000)] public decimal GrossMonthly { get; set; }
    [Range(1, 100)] public decimal BasicPercent { get; set; } = 40m;
    [Range(0, 100)] public decimal HRAPercent { get; set; } = 20m;
    [Range(0, 100)] public decimal SpecialAllowancePercent { get; set; } = 30m;
    public decimal ProfessionalTax { get; set; } = 200m;
}

public class UpdateEmployeeDto
{
    [MaxLength(100)] public string? FirstName { get; set; }
    [MaxLength(100)] public string? LastName { get; set; }
    [Phone] public string? Phone { get; set; }
    public Department? Department { get; set; }
    [MaxLength(150)] public string? Designation { get; set; }
    public EmploymentStatus? Status { get; set; }
    public string? PanNumber { get; set; }
    public string? Address { get; set; }
    public DateTime? ResignationDate { get; set; }
    public DateTime? LastWorkingDate { get; set; }
}

public class UpdateSalaryDto
{
    [Required, Range(1, 10000000)] public decimal GrossMonthly { get; set; }
    [Range(1, 100)] public decimal BasicPercent { get; set; } = 40m;
    [Range(0, 100)] public decimal HRAPercent { get; set; } = 20m;
    [Range(0, 100)] public decimal SpecialAllowancePercent { get; set; } = 30m;
    public decimal ProfessionalTax { get; set; } = 200m;
    public decimal IncomeTaxMonthly { get; set; } = 0m;
}

public class EmployeeFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public Department? Department { get; set; }
    public EmploymentStatus? Status { get; set; }
    public string? SortBy { get; set; } = "JoiningDate";
    public bool SortDescending { get; set; } = true;
}
