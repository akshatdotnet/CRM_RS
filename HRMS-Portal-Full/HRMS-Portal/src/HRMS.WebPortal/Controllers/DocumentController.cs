using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HRMS.Application.DTOs.Documents;
using HRMS.WebPortal.Models.Document;
using HRMS.WebPortal.Services;

namespace HRMS.WebPortal.Controllers;

[Authorize(Roles = "Admin,HR")]
public class DocumentController : Controller
{
    private readonly ApiClient _api;
    public DocumentController(ApiClient api) => _api = api;

    private string Token => User.FindFirstValue("AccessToken") ?? "";

    public async Task<IActionResult> Index(Guid employeeId, string? employeeName)
    {
        var docs = await _api.GetEmployeeDocumentsAsync(Token, employeeId);
        var vm = new DocumentListViewModel
        {
            EmployeeId = employeeId, EmployeeName = employeeName,
            Documents = (docs ?? Enumerable.Empty<DocumentDto>()).Select(d => new DocumentRowViewModel
            {
                Id = d.Id, EmployeeName = d.EmployeeName, Type = d.Type,
                Status = d.Status, EmailStatus = d.EmailStatus, GeneratedAt = d.GeneratedAt
            }).ToList()
        };
        return View(vm);
    }

    public async Task<IActionResult> GenerateOffer(Guid employeeId)
    {
        var emp = await _api.GetEmployeeAsync(Token, employeeId);
        return View(new GenerateOfferLetterViewModel
        {
            EmployeeId = employeeId, EmployeeName = emp?.FullName ?? "",
            DesignationOffered = emp?.Designation ?? ""
        });
    }

    [HttpPost] [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateOffer(GenerateOfferLetterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var dto = new OfferLetterRequestDto
        {
            DesignationOffered = vm.DesignationOffered, OfferedCTC = vm.OfferedCTC,
            JoiningDeadline = vm.JoiningDeadline, ReportingManager = vm.ReportingManager,
            WorkLocation = vm.WorkLocation, SpecialConditions = vm.SpecialConditions
        };
        var (result, error) = await _api.GenerateOfferLetterAsync(Token, vm.EmployeeId, dto);
        if (result != null)
        {
            TempData["Success"] = "Offer letter generated successfully.";
            return RedirectToAction("Index", new { employeeId = vm.EmployeeId, employeeName = vm.EmployeeName });
        }
        vm.ErrorMessage = error;
        return View(vm);
    }

    public async Task<IActionResult> GenerateAppointment(Guid employeeId, string? employeeName)
    {
        var (result, error) = await _api.GenerateAppointmentLetterAsync(Token, employeeId);
        TempData[result != null ? "Success" : "Error"] = result != null
            ? "Appointment letter generated." : error;
        return RedirectToAction("Index", new { employeeId, employeeName });
    }

    public async Task<IActionResult> GenerateExperience(Guid employeeId, string? employeeName)
    {
        var (result, error) = await _api.GenerateExperienceLetterAsync(Token, employeeId);
        TempData[result != null ? "Success" : "Error"] = result != null
            ? "Experience letter generated." : error;
        return RedirectToAction("Index", new { employeeId, employeeName });
    }

    public async Task<IActionResult> Download(Guid id)
    {
        var bytes = await _api.GetDocumentPdfAsync(Token, id);
        if (bytes == null) return NotFound();
        return File(bytes, "application/pdf", $"Document_{id}.pdf");
    }

    public async Task<IActionResult> Send(Guid id, Guid employeeId, string? employeeName)
    {
        await _api.SendDocumentAsync(Token, id);
        TempData["Success"] = "Document emailed to employee.";
        return RedirectToAction("Index", new { employeeId, employeeName });
    }
}
