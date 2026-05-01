using System.ComponentModel.DataAnnotations;
using HRMS.Domain.Enums;

namespace HRMS.WebPortal.Models.Employee;

public class EmployeeListViewModel
{
    public List<EmployeeRowViewModel> Employees { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public string? Search { get; set; }
    public string? DepartmentFilter { get; set; }
    public string? StatusFilter { get; set; }
}

public class EmployeeRowViewModel
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Department { get; set; } = "";
    public string Designation { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime JoiningDate { get; set; }
    public decimal? GrossMonthly { get; set; }
}

public class EmployeeDetailViewModel
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Department { get; set; } = "";
    public string Designation { get; set; } = "";
    public string Status { get; set; } = "";
    public string Gender { get; set; } = "";
    public DateTime JoiningDate { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? PanNumber { get; set; }
    public string? Address { get; set; }
    public decimal GrossMonthly { get; set; }
    public decimal NetMonthly { get; set; }
    public decimal AnnualCTC { get; set; }
}

public class CreateEmployeeViewModel
{
    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Phone { get; set; } = "";
    [Required] public Department Department { get; set; }
    [Required] public string Designation { get; set; } = "";
    [Required] public DateTime JoiningDate { get; set; } = DateTime.Today;
    [Required] public Gender Gender { get; set; }
    [Required] public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-25);
    public string? PanNumber { get; set; }
    public string? Address { get; set; }
    [Required] public UserRole RoleToAssign { get; set; } = UserRole.Employee;
    [Required, Range(1, 10_000_000)] public decimal GrossMonthly { get; set; } = 50000;
    [Range(1, 100)] public decimal BasicPercent { get; set; } = 40;
    [Range(0, 100)] public decimal HRAPercent { get; set; } = 20;
    [Range(0, 100)] public decimal SpecialAllowancePercent { get; set; } = 30;
    public decimal ProfessionalTax { get; set; } = 200;
    public string? ErrorMessage { get; set; }
}
