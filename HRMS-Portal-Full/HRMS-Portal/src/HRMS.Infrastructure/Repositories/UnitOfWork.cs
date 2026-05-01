using HRMS.Application.Interfaces.Repositories;
using HRMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace HRMS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly HrmsDbContext _ctx;
    private IDbContextTransaction? _transaction;

    public IEmployeeRepository Employees { get; }
    public IUserRepository Users { get; }
    public ISalarySlipRepository SalarySlips { get; }
    public IDocumentRepository Documents { get; }

    public UnitOfWork(HrmsDbContext ctx,
        IEmployeeRepository employees,
        IUserRepository users,
        ISalarySlipRepository salarySlips,
        IDocumentRepository documents)
    {
        _ctx = ctx;
        Employees = employees;
        Users = users;
        SalarySlips = salarySlips;
        Documents = documents;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _ctx.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _ctx.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _ctx.Dispose();
    }
}
