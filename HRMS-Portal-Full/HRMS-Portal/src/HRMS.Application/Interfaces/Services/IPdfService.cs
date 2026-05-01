namespace HRMS.Application.Interfaces.Services;

public interface IPdfService
{
    Task<byte[]> GeneratePdfAsync(string htmlContent, string title = "", CancellationToken ct = default);
    Task<byte[]> GenerateSalarySlipPdfAsync(object model, CancellationToken ct = default);
}
