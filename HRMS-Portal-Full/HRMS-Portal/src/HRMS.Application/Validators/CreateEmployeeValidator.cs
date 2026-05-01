using FluentValidation;
using HRMS.Application.DTOs.Employee;

namespace HRMS.Application.Validators;

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("First name contains invalid characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .Matches(@"^[+]?[\d\s\-\(\)]{7,20}$").WithMessage("Invalid phone number format.");

        RuleFor(x => x.Designation)
            .NotEmpty().MaximumLength(150);

        RuleFor(x => x.JoiningDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(90)).WithMessage("Joining date cannot be more than 90 days in the future.")
            .GreaterThan(new DateTime(2000, 1, 1)).WithMessage("Joining date seems too far in the past.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.UtcNow.AddYears(-18)).WithMessage("Employee must be at least 18 years old.")
            .GreaterThan(DateTime.UtcNow.AddYears(-70)).WithMessage("Please verify the date of birth.");

        RuleFor(x => x.PanNumber)
            .Matches(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$").WithMessage("Invalid PAN format (e.g., ABCDE1234F).")
            .When(x => !string.IsNullOrEmpty(x.PanNumber));

        RuleFor(x => x.Salary).NotNull().WithMessage("Salary structure is required.");
        RuleFor(x => x.Salary.GrossMonthly)
            .GreaterThan(0).WithMessage("Gross monthly must be positive.")
            .LessThanOrEqualTo(10_000_000).WithMessage("Gross monthly exceeds maximum allowed value.");

        RuleFor(x => x.Salary.BasicPercent + x.Salary.HRAPercent + x.Salary.SpecialAllowancePercent)
            .LessThanOrEqualTo(100).WithMessage("Sum of Basic%, HRA%, and Special Allowance% cannot exceed 100%.")
            .When(x => x.Salary != null);
    }
}
