using HRMS.Application.DTOs.Documents;
using HRMS.Domain.Enums;

namespace HRMS.Application.Interfaces.Services;

public interface IDocumentService
{
    Task<DocumentDto> GenerateOfferLetterAsync(Guid employeeId, OfferLetterRequestDto request, string createdBy, CancellationToken ct = default);
    Task<DocumentDto> GenerateAppointmentLetterAsync(Guid employeeId, string createdBy, CancellationToken ct = default);
    Task<DocumentDto> GenerateExperienceLetterAsync(Guid employeeId, string createdBy, CancellationToken ct = default);
    Task<DocumentDto> GenerateForm16Async(Guid employeeId, int financialYear, string createdBy, CancellationToken ct = default);
    Task<byte[]> GetDocumentPdfAsync(Guid documentId, CancellationToken ct = default);
    Task<bool> SendDocumentEmailAsync(Guid documentId, string sentBy, CancellationToken ct = default);
    Task<IEnumerable<DocumentDto>> GetEmployeeDocumentsAsync(Guid employeeId, CancellationToken ct = default);
}
