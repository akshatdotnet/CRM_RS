namespace HRMS.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IEmployeeRepository Employees { get; }
    IUserRepository Users { get; }
    ISalarySlipRepository SalarySlips { get; }
    IDocumentRepository Documents { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
