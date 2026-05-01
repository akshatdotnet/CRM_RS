using FluentValidation;
using HRMS.Application.DTOs.Salary;

namespace HRMS.Application.Validators;

public class GenerateSalarySlipValidator : AbstractValidator<GenerateSalarySlipDto>
{
    public GenerateSalarySlipValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required.");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");

        RuleFor(x => x.Year)
            .InclusiveBetween(2020, DateTime.UtcNow.Year + 1)
            .WithMessage($"Year must be between 2020 and {DateTime.UtcNow.Year + 1}.");

        RuleFor(x => x)
            .Must(x => !(x.Year == DateTime.UtcNow.Year && x.Month > DateTime.UtcNow.Month))
            .WithMessage("Cannot generate salary slip for a future month.");

        RuleFor(x => x.PaidDays)
            .GreaterThan(0).WithMessage("Paid days must be greater than 0.")
            .When(x => x.PaidDays.HasValue);

        RuleFor(x => x.OtherAllowances)
            .GreaterThanOrEqualTo(0).WithMessage("Other allowances cannot be negative.");

        RuleFor(x => x.OtherDeductions)
            .GreaterThanOrEqualTo(0).WithMessage("Other deductions cannot be negative.");
    }
}
