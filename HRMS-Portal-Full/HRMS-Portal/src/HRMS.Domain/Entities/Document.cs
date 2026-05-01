using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class Document : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public DocumentType Type { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public string TemplateSnapshot { get; set; } = string.Empty; // Store rendered HTML/text
    public string? PdfPath { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public EmailStatus EmailStatus { get; set; } = EmailStatus.Pending;
    public string? EmailError { get; set; }

    // For salary slip / form 16 - month/year context
    public int? ForMonth { get; set; }
    public int? ForYear { get; set; }
}
