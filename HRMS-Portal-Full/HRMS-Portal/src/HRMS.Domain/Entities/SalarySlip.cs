using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class SalarySlip : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int Month { get; set; }
    public int Year { get; set; }
    public int PaidDays { get; set; }
    public int TotalDays { get; set; }

    // Earnings
    public decimal Basic { get; set; }
    public decimal HRA { get; set; }
    public decimal SpecialAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossEarnings => Basic + HRA + SpecialAllowance + OtherAllowances;

    // Deductions
    public decimal EmployeePF { get; set; }
    public decimal ProfessionalTax { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions => EmployeePF + ProfessionalTax + IncomeTax + OtherDeductions;

    public decimal NetPay => GrossEarnings - TotalDeductions;

    public string? PdfPath { get; set; }
    public bool IsSent { get; set; } = false;

    public string MonthYear => $"{new DateTime(Year, Month, 1):MMMM yyyy}";
}
