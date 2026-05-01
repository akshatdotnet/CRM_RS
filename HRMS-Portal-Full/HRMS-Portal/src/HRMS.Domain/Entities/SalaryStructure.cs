using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class SalaryStructure : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public decimal GrossMonthly { get; set; }
    public decimal BasicPercent { get; set; } = 40m;    // % of gross
    public decimal HRAPercent { get; set; } = 20m;      // % of gross
    public decimal SpecialAllowancePercent { get; set; } = 30m;
    public decimal PFPercent { get; set; } = 12m;        // % of basic (employer + employee)
    public decimal ProfessionalTax { get; set; } = 200m; // Fixed monthly (state-dependent)
    public decimal IncomeTaxMonthly { get; set; } = 0m;  // Computed/overridable

    // Computed properties
    public decimal Basic => GrossMonthly * (BasicPercent / 100);
    public decimal HRA => GrossMonthly * (HRAPercent / 100);
    public decimal SpecialAllowance => GrossMonthly * (SpecialAllowancePercent / 100);
    public decimal EmployeePF => Basic * (PFPercent / 100);
    public decimal EmployerPF => Basic * (PFPercent / 100);

    public decimal NetMonthly =>
        GrossMonthly - EmployeePF - ProfessionalTax - IncomeTaxMonthly;

    public decimal AnnualCTC => (GrossMonthly + EmployerPF) * 12;

    public DateTime EffectiveFrom { get; set; }
    public bool IsCurrent { get; set; } = true;
}
