using HRMS.Application.Common;
using HRMS.Application.DTOs.Salary;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalarySlipsController : ControllerBase
{
    private readonly ISalarySlipService _svc;
    public SalarySlipsController(ISalarySlipService svc) => _svc = svc;

    private string CurrentUser => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

    /// <summary>Generate salary slip for an employee. HR/Admin only.</summary>
    [HttpPost]
    [Authorize(Policy = "HROrAdmin")]
    public async Task<ActionResult<ApiResponse<SalarySlipDto>>> Generate(
        [FromBody] GenerateSalarySlipDto dto, CancellationToken ct)
    {
        var slip = await _svc.GenerateSlipAsync(dto, CurrentUser, ct);
        return Ok(ApiResponse<SalarySlipDto>.Ok(slip, "Salary slip generated."));
    }

    /// <summary>Bulk generate salary slips for all active employees.</summary>
    [HttpPost("bulk")]
    [Authorize(Policy = "HROrAdmin")]
    public async Task<IActionResult> BulkGenerate([FromQuery] int month, [FromQuery] int year, CancellationToken ct)
    {
        await _svc.BulkGenerateAsync(month, year, CurrentUser, ct);
        return Ok(new { message = $"Bulk generation complete for {month}/{year}." });
    }

    /// <summary>Get salary slip by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SalarySlipDto>>> GetById(Guid id, CancellationToken ct)
    {
        var slip = await _svc.GetSlipAsync(id, ct);
        return Ok(ApiResponse<SalarySlipDto>.Ok(slip));
    }

    /// <summary>Get all salary slips for an employee.</summary>
    [HttpGet("employee/{employeeId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SalarySlipDto>>>> GetByEmployee(
        Guid employeeId, CancellationToken ct)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Employee")
        {
            var empId = User.FindFirstValue("employeeId");
            if (empId != employeeId.ToString()) return Forbid();
        }
        var slips = await _svc.GetEmployeeSlipsAsync(employeeId, ct);
        return Ok(ApiResponse<IEnumerable<SalarySlipDto>>.Ok(slips));
    }

    /// <summary>Download salary slip as PDF.</summary>
    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> GetPdf(Guid id, CancellationToken ct)
    {
        var bytes = await _svc.GetSlipPdfAsync(id, ct);
        return File(bytes, "application/pdf", $"SalarySlip_{id}.pdf");
    }

    /// <summary>Email salary slip to employee. HR/Admin only.</summary>
    [HttpPost("{id:guid}/send")]
    [Authorize(Policy = "HROrAdmin")]
    public async Task<IActionResult> Send(Guid id, CancellationToken ct)
    {
        var sent = await _svc.SendSlipEmailAsync(id, CurrentUser, ct);
        return Ok(new { success = sent, message = sent ? "Slip emailed successfully." : "Email delivery failed." });
    }
}
