using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Salary;

public class SalarySlipDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public string Designation { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string? PanNumber { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }
    public string MonthYear { get; init; } = string.Empty;
    public int PaidDays { get; init; }
    public int TotalDays { get; init; }
    public decimal Basic { get; init; }
    public decimal HRA { get; init; }
    public decimal SpecialAllowance { get; init; }
    public decimal OtherAllowances { get; init; }
    public decimal GrossEarnings { get; init; }
    public decimal EmployeePF { get; init; }
    public decimal ProfessionalTax { get; init; }
    public decimal IncomeTax { get; init; }
    public decimal OtherDeductions { get; init; }
    public decimal TotalDeductions { get; init; }
    public decimal NetPay { get; init; }
    public bool IsSent { get; init; }
}

public class GenerateSalarySlipDto
{
    [Required] public Guid EmployeeId { get; set; }
    [Required, Range(1, 12)] public int Month { get; set; }
    [Required, Range(2020, 2100)] public int Year { get; set; }
    public int? PaidDays { get; set; } // null = full month
    public decimal OtherAllowances { get; set; } = 0;
    public decimal OtherDeductions { get; set; } = 0;
}
