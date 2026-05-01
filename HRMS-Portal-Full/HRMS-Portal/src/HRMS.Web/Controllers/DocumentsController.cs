using HRMS.Application.Common;
using HRMS.Application.DTOs.Documents;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _svc;
    public DocumentsController(IDocumentService svc) => _svc = svc;

    private string CurrentUser => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

    /// <summary>Get all documents for an employee.</summary>
    [HttpGet("employee/{employeeId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DocumentDto>>>> GetEmployeeDocuments(
        Guid employeeId, CancellationToken ct)
    {
        var docs = await _svc.GetEmployeeDocumentsAsync(employeeId, ct);
        return Ok(ApiResponse<IEnumerable<DocumentDto>>.Ok(docs));
    }

    /// <summary>Generate an offer letter. HR/Admin only.</summary>
    [HttpPost("employee/{employeeId:guid}/offer-letter")]
    [Authorize(Policy = "HROrAdmin")]
    public async Task<ActionResult<ApiResponse<DocumentDto>>> GenerateOfferLetter(
        Guid employeeId, [FromBody] OfferLetterRequestDto request, CancellationToken ct)
    {
        var doc = await _svc.GenerateOfferLetterAsync(employeeId, request, CurrentUser, ct);
        return Ok(ApiResponse<DocumentDto>.Ok(doc, "Offer letter generated."));
    }

    /// <summary>Generate an appointment letter. HR/Admin only.</summary>
    [HttpPost("employee/{employeeId:guid}/appointment-letter")]
    [Authorize(Policy = "HROrAdmin")]
    public async Task<ActionResult<ApiResponse<DocumentDto>>> GenerateAppointmentLetter(
        Guid employeeId, CancellationToken ct)
    {
        var doc = await _svc.GenerateAppointmentLetterAsync(employeeId, CurrentUser, ct);
        return Ok(ApiResponse<DocumentDto>.Ok(doc, "Appointment letter generated."));
    }

    /// <summary>Generate experience letter (employee must be separated). HR/Admin only.</summary>
    [HttpPost("employee/{employeeId:guid}/experience-letter")]
    [Authorize(Policy = "HROrAdmin")]
    public async Task<ActionResult<ApiResponse<DocumentDto>>> GenerateExperienceLetter(
        Guid employeeId, CancellationToken ct)
    {
        var doc = await _svc.GenerateExperienceLetterAsync(employeeId, CurrentUser, ct);
        return Ok(ApiResponse<DocumentDto>.Ok(doc, "Experience letter generated."));
    }

    /// <summary>Generate Form 16 for an employee. HR/Admin only.</summary>
    [HttpPost("employee/{employeeId:guid}/form16")]
    [Authorize(Policy = "HROrAdmin")]
    public async Task<ActionResult<ApiResponse<DocumentDto>>> GenerateForm16(
        Guid employeeId, [FromQuery] int financialYear, CancellationToken ct)
    {
        var doc = await _svc.GenerateForm16Async(employeeId, financialYear, CurrentUser, ct);
        return Ok(ApiResponse<DocumentDto>.Ok(doc, "Form 16 generated."));
    }

    /// <summary>Download document as PDF.</summary>
    [HttpGet("{documentId:guid}/pdf")]
    public async Task<IActionResult> GetPdf(Guid documentId, CancellationToken ct)
    {
        var bytes = await _svc.GetDocumentPdfAsync(documentId, ct);
        return File(bytes, "application/pdf", $"Document_{documentId}.pdf");
    }

    /// <summary>Email document to employee. HR/Admin only.</summary>
    [HttpPost("{documentId:guid}/send")]
    [Authorize(Policy = "HROrAdmin")]
    public async Task<IActionResult> Send(Guid documentId, CancellationToken ct)
    {
        var sent = await _svc.SendDocumentEmailAsync(documentId, CurrentUser, ct);
        return Ok(new { success = sent, message = sent ? "Document emailed." : "Email delivery failed." });
    }
}
