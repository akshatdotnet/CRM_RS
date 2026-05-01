using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HRMS.Application.DTOs.Employee;
using HRMS.Domain.Enums;
using HRMS.WebPortal.Models.Employee;
using HRMS.WebPortal.Services;

namespace HRMS.WebPortal.Controllers;

[Authorize]
public class EmployeeController : Controller
{
    private readonly ApiClient _api;
    public EmployeeController(ApiClient api) => _api = api;

    private string Token => User.FindFirstValue("AccessToken") ?? "";

    // GET /Employee
    public async Task<IActionResult> Index(int page = 1, string? search = null,
        string? dept = null, string? status = null)
    {
        var result = await _api.GetEmployeesAsync(Token, page, 15, search, dept, status);
        var vm = new EmployeeListViewModel
        {
            Employees = result?.Items.Select(e => new EmployeeRowViewModel
            {
                Id = e.Id, EmployeeCode = e.EmployeeCode, FullName = e.FullName,
                Email = e.Email, Department = e.Department, Designation = e.Designation,
                Status = e.Status, JoiningDate = e.JoiningDate, GrossMonthly = e.GrossMonthly
            }).ToList() ?? new(),
            Page       = result?.Page      ?? 1,
            PageSize   = result?.PageSize  ?? 15,
            TotalCount = result?.TotalCount ?? 0,
            TotalPages = result?.TotalPages ?? 1,
            Search           = search,
            DepartmentFilter = dept,
            StatusFilter     = status
        };
        return View(vm);
    }

    // GET /Employee/Detail/{id}
    public async Task<IActionResult> Detail(Guid id)
    {
        // Employees can only see their own record
        var role  = User.FindFirstValue(ClaimTypes.Role);
        var empId = User.FindFirstValue("employeeId");
        if (role == "Employee" && empId != id.ToString())
            return RedirectToAction("AccessDenied", "Auth");

        var emp = await _api.GetEmployeeAsync(Token, id);
        if (emp == null) return NotFound();

        var vm = new EmployeeDetailViewModel
        {
            Id = emp.Id, EmployeeCode = emp.EmployeeCode, FullName = emp.FullName,
            Email = emp.Email, Phone = emp.Phone, Department = emp.Department,
            Designation = emp.Designation, Status = emp.Status, Gender = emp.Gender,
            JoiningDate = emp.JoiningDate, DateOfBirth = emp.DateOfBirth,
            PanNumber = emp.PanNumber, Address = emp.Address,
            GrossMonthly = emp.SalaryStructure?.GrossMonthly ?? 0,
            NetMonthly   = emp.SalaryStructure?.NetMonthly   ?? 0,
            AnnualCTC    = emp.SalaryStructure?.AnnualCTC    ?? 0,
        };
        return View(vm);
    }

    // GET /Employee/Create
    [Authorize(Roles = "Admin,HR")]
    public IActionResult Create()
    {
        return View(new CreateEmployeeViewModel
        {
            JoiningDate = DateTime.Today,
            DateOfBirth = DateTime.Today.AddYears(-25),
            GrossMonthly = 50000m,
            BasicPercent = 40m, HRAPercent = 20m, SpecialAllowancePercent = 30m,
            ProfessionalTax = 200m, RoleToAssign = UserRole.Employee
        });
    }

    // POST /Employee/Create
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Create(CreateEmployeeViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var dto = new CreateEmployeeDto
        {
            FirstName   = vm.FirstName, LastName     = vm.LastName,
            Email       = vm.Email,     Phone        = vm.Phone,
            Department  = vm.Department, Designation = vm.Designation,
            JoiningDate = vm.JoiningDate, Gender     = vm.Gender,
            DateOfBirth = vm.DateOfBirth, PanNumber  = vm.PanNumber,
            Address     = vm.Address,  RoleToAssign  = vm.RoleToAssign,
            Salary = new CreateSalaryDto
            {
                GrossMonthly             = vm.GrossMonthly,
                BasicPercent             = vm.BasicPercent,
                HRAPercent               = vm.HRAPercent,
                SpecialAllowancePercent  = vm.SpecialAllowancePercent,
                ProfessionalTax          = vm.ProfessionalTax
            }
        };

        var (result, error) = await _api.CreateEmployeeAsync(Token, dto);
        if (result != null)
        {
            TempData["Success"] = $"Employee {result.FullName} ({result.EmployeeCode}) created. Default password: Welcome@123";
            return RedirectToAction("Detail", new { id = result.Id });
        }

        vm.ErrorMessage = error ?? "Failed to create employee.";
        return View(vm);
    }
}
