using System.ComponentModel.DataAnnotations;

namespace HRMS.WebPortal.Models.Salary;

public class SalarySlipListViewModel
{
    public Guid? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public List<SalarySlipRowViewModel> Slips { get; set; } = new();
}

public class SalarySlipRowViewModel
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = "";
    public string EmployeeName { get; set; } = "";
    public string MonthYear { get; set; } = "";
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal GrossEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
    public bool IsSent { get; set; }
}

public class GenerateSalarySlipViewModel
{
    [Required] public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";
    [Required, Range(1, 12)] public int Month { get; set; } = DateTime.Now.Month;
    [Required, Range(2020, 2100)] public int Year { get; set; } = DateTime.Now.Year;
    public int? PaidDays { get; set; }
    [Range(0, double.MaxValue)] public decimal OtherAllowances { get; set; } = 0;
    [Range(0, double.MaxValue)] public decimal OtherDeductions { get; set; } = 0;
    public string? ErrorMessage { get; set; }
}
