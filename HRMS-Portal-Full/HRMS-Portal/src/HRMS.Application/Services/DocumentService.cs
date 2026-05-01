using AutoMapper;
using HRMS.Application.DTOs.Documents;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IPdfService _pdf;
    private readonly IEmailService _email;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(IUnitOfWork uow, IMapper mapper, IPdfService pdf,
        IEmailService email, ILogger<DocumentService> logger)
    {
        _uow = uow; _mapper = mapper; _pdf = pdf; _email = email; _logger = logger;
    }

    public async Task<DocumentDto> GenerateOfferLetterAsync(Guid employeeId, OfferLetterRequestDto request, string createdBy, CancellationToken ct = default)
    {
        var employee = await _uow.Employees.GetWithSalaryAsync(employeeId, ct)
            ?? throw new KeyNotFoundException($"Employee {employeeId} not found.");

        var html = DocumentTemplates.OfferLetter(employee, request);
        return await CreateDocumentAsync(employee, DocumentType.OfferLetter, html, createdBy, ct);
    }

    public async Task<DocumentDto> GenerateAppointmentLetterAsync(Guid employeeId, string createdBy, CancellationToken ct = default)
    {
        var employee = await _uow.Employees.GetWithSalaryAsync(employeeId, ct)
            ?? throw new KeyNotFoundException($"Employee {employeeId} not found.");

        var html = DocumentTemplates.AppointmentLetter(employee);
        return await CreateDocumentAsync(employee, DocumentType.AppointmentLetter, html, createdBy, ct);
    }

    public async Task<DocumentDto> GenerateExperienceLetterAsync(Guid employeeId, string createdBy, CancellationToken ct = default)
    {
        var employee = await _uow.Employees.GetWithSalaryAsync(employeeId, ct)
            ?? throw new KeyNotFoundException($"Employee {employeeId} not found.");

        if (employee.Status == EmploymentStatus.Active)
            throw new InvalidOperationException("Experience letter can only be generated after separation.");

        var html = DocumentTemplates.ExperienceLetter(employee);
        return await CreateDocumentAsync(employee, DocumentType.ExperienceLetter, html, createdBy, ct);
    }

    public async Task<DocumentDto> GenerateForm16Async(Guid employeeId, int financialYear, string createdBy, CancellationToken ct = default)
    {
        var employee = await _uow.Employees.GetWithSalaryAsync(employeeId, ct)
            ?? throw new KeyNotFoundException($"Employee {employeeId} not found.");

        // Aggregate annual salary from slips for the FY (Apr-Mar)
        var slips = await _uow.SalarySlips.GetByEmployeeAsync(employeeId, ct);
        var fySlips = slips.Where(s =>
            (s.Year == financialYear && s.Month >= 4) ||
            (s.Year == financialYear + 1 && s.Month <= 3)).ToList();

        var html = DocumentTemplates.Form16(employee, fySlips, financialYear);
        return await CreateDocumentAsync(employee, DocumentType.Form16, html, createdBy, ct);
    }

    public async Task<byte[]> GetDocumentPdfAsync(Guid documentId, CancellationToken ct = default)
    {
        var doc = await _uow.Documents.GetByIdAsync(documentId, ct)
            ?? throw new KeyNotFoundException($"Document {documentId} not found.");

        return await _pdf.GeneratePdfAsync(doc.TemplateSnapshot, doc.Type.ToString(), ct);
    }

    public async Task<bool> SendDocumentEmailAsync(Guid documentId, string sentBy, CancellationToken ct = default)
    {
        var doc = await _uow.Documents.GetByIdAsync(documentId, ct)
            ?? throw new KeyNotFoundException($"Document {documentId} not found.");

        var employee = await _uow.Employees.GetByIdAsync(doc.EmployeeId, ct)!
            ?? throw new KeyNotFoundException("Employee not found.");

        var pdfBytes = await GetDocumentPdfAsync(documentId, ct);
        var subject = doc.Type switch
        {
            DocumentType.OfferLetter => "Your Offer Letter",
            DocumentType.AppointmentLetter => "Your Appointment Letter",
            DocumentType.ExperienceLetter => "Your Experience Letter",
            DocumentType.Form16 => $"Form 16 - FY {doc.ForYear}-{doc.ForYear! + 1}",
            _ => "HR Document"
        };

        var sent = await _email.SendWithAttachmentAsync(
            new Interfaces.Services.EmailMessage(
                employee.Email, employee.FullName, subject,
                $"<p>Dear {employee.FirstName},</p><p>Please find your {doc.Type} attached.</p><p>Regards,<br/>HR Team</p>"
            ),
            pdfBytes,
            $"{doc.Type}_{employee.EmployeeCode}_{DateTime.UtcNow:yyyyMMdd}.pdf",
            ct);

        doc.EmailStatus = sent ? EmailStatus.Sent : EmailStatus.Failed;
        doc.SentAt = sent ? DateTime.UtcNow : null;
        doc.SetAuditOnUpdate(sentBy);
        await _uow.Documents.UpdateAsync(doc, ct);
        await _uow.SaveChangesAsync(ct);

        return sent;
    }

    public async Task<IEnumerable<DocumentDto>> GetEmployeeDocumentsAsync(Guid employeeId, CancellationToken ct = default)
    {
        var docs = await _uow.Documents.GetByEmployeeAsync(employeeId, ct);
        return _mapper.Map<IEnumerable<DocumentDto>>(docs);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<DocumentDto> CreateDocumentAsync(Employee employee, DocumentType type,
        string html, string createdBy, CancellationToken ct)
    {
        var doc = new Document
        {
            EmployeeId = employee.Id,
            Employee = employee,
            Type = type,
            TemplateSnapshot = html,
            Status = DocumentStatus.Generated,
            GeneratedAt = DateTime.UtcNow,
            EmailStatus = EmailStatus.Pending
        };
        doc.SetAuditOnCreate(createdBy);

        await _uow.Documents.AddAsync(doc, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("{Type} generated for {Code}", type, employee.EmployeeCode);
        return _mapper.Map<DocumentDto>(doc);
    }
}
