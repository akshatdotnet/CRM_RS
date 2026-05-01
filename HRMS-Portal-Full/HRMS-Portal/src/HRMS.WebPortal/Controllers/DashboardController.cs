using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HRMS.WebPortal.Models.Dashboard;
using HRMS.WebPortal.Services;

namespace HRMS.WebPortal.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApiClient _api;
    public DashboardController(ApiClient api) => _api = api;

    private string Token => User.FindFirstValue("AccessToken") ?? "";

    public async Task<IActionResult> Index()
    {
        var emps = await _api.GetEmployeesAsync(Token, 1, 100);

        var vm = new DashboardViewModel
        {
            UserName = User.FindFirstValue(ClaimTypes.Name) ?? "",
            UserRole = User.FindFirstValue(ClaimTypes.Role) ?? "",
            UserEmail = User.FindFirstValue(ClaimTypes.Email) ?? "",
            TotalEmployees = emps?.TotalCount ?? 0,
            ActiveEmployees = emps?.Items.Count(e => e.Status == "Active") ?? 0,
            RecentEmployees = emps?.Items.Take(6).Select(e => new EmployeeSummary
            {
                Id = e.Id, EmployeeCode = e.EmployeeCode, FullName = e.FullName,
                Department = e.Department, Designation = e.Designation,
                Status = e.Status, JoiningDate = e.JoiningDate
            }).ToList() ?? new()
        };
        return View(vm);
    }
}
