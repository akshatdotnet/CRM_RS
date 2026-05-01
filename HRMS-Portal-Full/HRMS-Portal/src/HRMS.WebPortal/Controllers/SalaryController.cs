using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HRMS.Application.DTOs.Salary;
using HRMS.WebPortal.Models.Salary;
using HRMS.WebPortal.Services;

namespace HRMS.WebPortal.Controllers;

[Authorize]
public class SalaryController : Controller
{
    private readonly ApiClient _api;
    public SalaryController(ApiClient api) => _api = api;

    private string Token => User.FindFirstValue("AccessToken") ?? "";

    public async Task<IActionResult> Index(Guid? employeeId, string? employeeName)
    {
        var slips = employeeId.HasValue
            ? await _api.GetEmployeeSlipsAsync(Token, employeeId.Value)
            : Enumerable.Empty<SalarySlipDto>();

        var vm = new SalarySlipListViewModel
        {
            EmployeeId = employeeId, EmployeeName = employeeName,
            Slips = (slips ?? Enumerable.Empty<SalarySlipDto>()).Select(s => new SalarySlipRowViewModel
            {
                Id = s.Id, EmployeeCode = s.EmployeeCode, EmployeeName = s.EmployeeName,
                MonthYear = s.MonthYear, Month = s.Month, Year = s.Year,
                GrossEarnings = s.GrossEarnings, TotalDeductions = s.TotalDeductions,
                NetPay = s.NetPay, IsSent = s.IsSent
            }).ToList()
        };
        return View(vm);
    }

    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Generate(Guid employeeId)
    {
        var emp = await _api.GetEmployeeAsync(Token, employeeId);
        return View(new GenerateSalarySlipViewModel
        {
            EmployeeId = employeeId,
            EmployeeName = emp?.FullName ?? ""
        });
    }

    [HttpPost] [ValidateAntiForgeryToken] [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Generate(GenerateSalarySlipViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var dto = new GenerateSalarySlipDto
        {
            EmployeeId = vm.EmployeeId, Month = vm.Month, Year = vm.Year,
            PaidDays = vm.PaidDays, OtherAllowances = vm.OtherAllowances,
            OtherDeductions = vm.OtherDeductions
        };
        var (result, error) = await _api.GenerateSalarySlipAsync(Token, dto);
        if (result != null)
        {
            TempData["Success"] = $"Salary slip generated for {result.MonthYear}";
            return RedirectToAction("Index", new { employeeId = vm.EmployeeId, employeeName = vm.EmployeeName });
        }
        vm.ErrorMessage = error;
        return View(vm);
    }

    public async Task<IActionResult> Download(Guid id)
    {
        var bytes = await _api.GetSlipPdfAsync(Token, id);
        if (bytes == null) return NotFound();
        return File(bytes, "application/pdf", $"SalarySlip_{id}.pdf");
    }

    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Send(Guid id, Guid employeeId, string? employeeName)
    {
        await _api.SendSlipAsync(Token, id);
        TempData["Success"] = "Salary slip emailed to employee.";
        return RedirectToAction("Index", new { employeeId, employeeName });
    }
}
