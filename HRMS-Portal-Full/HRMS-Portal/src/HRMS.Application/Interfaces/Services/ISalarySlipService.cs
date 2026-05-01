using HRMS.Application.DTOs.Salary;

namespace HRMS.Application.Interfaces.Services;

public interface ISalarySlipService
{
    Task<SalarySlipDto> GenerateSlipAsync(GenerateSalarySlipDto request, string createdBy, CancellationToken ct = default);
    Task<SalarySlipDto> GetSlipAsync(Guid slipId, CancellationToken ct = default);
    Task<IEnumerable<SalarySlipDto>> GetEmployeeSlipsAsync(Guid employeeId, CancellationToken ct = default);
    Task<byte[]> GetSlipPdfAsync(Guid slipId, CancellationToken ct = default);
    Task<bool> SendSlipEmailAsync(Guid slipId, string sentBy, CancellationToken ct = default);
    Task BulkGenerateAsync(int month, int year, string createdBy, CancellationToken ct = default);
}
