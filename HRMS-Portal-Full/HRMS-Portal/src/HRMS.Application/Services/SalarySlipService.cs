using AutoMapper;
using HRMS.Application.DTOs.Salary;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Services;

public class SalarySlipService : ISalarySlipService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IPdfService _pdf;
    private readonly IEmailService _email;
    private readonly ILogger<SalarySlipService> _logger;

    public SalarySlipService(IUnitOfWork uow, IMapper mapper, IPdfService pdf,
        IEmailService email, ILogger<SalarySlipService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _pdf = pdf;
        _email = email;
        _logger = logger;
    }

    public async Task<SalarySlipDto> GenerateSlipAsync(GenerateSalarySlipDto request, string createdBy, CancellationToken ct = default)
    {
        var employee = await _uow.Employees.GetWithSalaryAsync(request.EmployeeId, ct)
            ?? throw new KeyNotFoundException($"Employee {request.EmployeeId} not found.");

        if (employee.SalaryStructure == null)
            throw new InvalidOperationException("Employee has no salary structure configured.");

        var existing = await _uow.SalarySlips.GetByEmployeeMonthYearAsync(request.EmployeeId, request.Month, request.Year, ct);
        if (existing != null)
            throw new InvalidOperationException($"Salary slip for {new DateTime(request.Year, request.Month, 1):MMMM yyyy} already exists.");

        var sal = employee.SalaryStructure;
        int totalDays = DateTime.DaysInMonth(request.Year, request.Month);
        int paidDays = request.PaidDays ?? totalDays;
        decimal ratio = (decimal)paidDays / totalDays;

        var slip = new SalarySlip
        {
            EmployeeId = employee.Id,
            Month = request.Month,
            Year = request.Year,
            PaidDays = paidDays,
            TotalDays = totalDays,
            Basic = Math.Round(sal.Basic * ratio, 2),
            HRA = Math.Round(sal.HRA * ratio, 2),
            SpecialAllowance = Math.Round(sal.SpecialAllowance * ratio, 2),
            OtherAllowances = request.OtherAllowances,
            EmployeePF = Math.Round(sal.EmployeePF * ratio, 2),
            ProfessionalTax = paidDays == totalDays ? sal.ProfessionalTax : 0m, // PT only for full month
            IncomeTax = Math.Round(sal.IncomeTaxMonthly * ratio, 2),
            OtherDeductions = request.OtherDeductions
        };
        slip.SetAuditOnCreate(createdBy);

        // Attach employee nav for mapper
        slip.Employee = employee;

        await _uow.SalarySlips.AddAsync(slip, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Salary slip generated for {Code} - {MonthYear}", employee.EmployeeCode, slip.MonthYear);
        return _mapper.Map<SalarySlipDto>(slip);
    }

    public async Task<SalarySlipDto> GetSlipAsync(Guid slipId, CancellationToken ct = default)
    {
        var slip = await _uow.SalarySlips.GetByIdAsync(slipId, ct)
            ?? throw new KeyNotFoundException($"Salary slip {slipId} not found.");
        return _mapper.Map<SalarySlipDto>(slip);
    }

    public async Task<IEnumerable<SalarySlipDto>> GetEmployeeSlipsAsync(Guid employeeId, CancellationToken ct = default)
    {
        var slips = await _uow.SalarySlips.GetByEmployeeAsync(employeeId, ct);
        return _mapper.Map<IEnumerable<SalarySlipDto>>(slips);
    }

    public async Task<byte[]> GetSlipPdfAsync(Guid slipId, CancellationToken ct = default)
    {
        var slip = await _uow.SalarySlips.GetByIdAsync(slipId, ct)
            ?? throw new KeyNotFoundException($"Salary slip {slipId} not found.");

        var dto = _mapper.Map<SalarySlipDto>(slip);
        var html = SalarySlipTemplate.Render(dto);
        return await _pdf.GeneratePdfAsync(html, $"Salary Slip - {dto.MonthYear}", ct);
    }

    public async Task<bool> SendSlipEmailAsync(Guid slipId, string sentBy, CancellationToken ct = default)
    {
        var slip = await _uow.SalarySlips.GetByIdAsync(slipId, ct)
            ?? throw new KeyNotFoundException($"Salary slip {slipId} not found.");

        var employee = await _uow.Employees.GetByIdAsync(slip.EmployeeId, ct)!
            ?? throw new KeyNotFoundException("Employee not found.");

        var pdfBytes = await GetSlipPdfAsync(slipId, ct);
        var dto = _mapper.Map<SalarySlipDto>(slip);

        var sent = await _email.SendWithAttachmentAsync(
            new Interfaces.Services.EmailMessage(
                employee.Email,
                employee.FullName,
                $"Salary Slip for {dto.MonthYear}",
                $"<p>Dear {employee.FirstName},</p><p>Please find your salary slip for {dto.MonthYear} attached.</p><p>Regards,<br/>HR Team</p>"
            ),
            pdfBytes,
            $"SalarySlip_{employee.EmployeeCode}_{dto.MonthYear.Replace(" ", "_")}.pdf",
            ct);

        if (sent)
        {
            slip.IsSent = true;
            slip.SetAuditOnUpdate(sentBy);
            await _uow.SalarySlips.UpdateAsync(slip, ct);
            await _uow.SaveChangesAsync(ct);
        }

        return sent;
    }

    public async Task BulkGenerateAsync(int month, int year, string createdBy, CancellationToken ct = default)
    {
        var employees = await _uow.Employees.GetActiveEmployeesAsync(ct);
        int count = 0;

        foreach (var emp in employees)
        {
            var exists = await _uow.SalarySlips.GetByEmployeeMonthYearAsync(emp.Id, month, year, ct);
            if (exists != null) continue;

            try
            {
                await GenerateSlipAsync(new GenerateSalarySlipDto
                {
                    EmployeeId = emp.Id,
                    Month = month,
                    Year = year
                }, createdBy, ct);
                count++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate slip for {Code}", emp.EmployeeCode);
            }
        }

        _logger.LogInformation("Bulk salary slips: {Count} generated for {Month}/{Year}", count, month, year);
    }
}
