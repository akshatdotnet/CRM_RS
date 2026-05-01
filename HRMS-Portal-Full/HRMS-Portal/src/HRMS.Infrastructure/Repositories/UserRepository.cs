using HRMS.Application.Interfaces.Repositories;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(HrmsDbContext ctx) : base(ctx) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<User?> GetByRefreshTokenAsync(string token, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(u => u.RefreshToken == token, ct);

    public async Task<User?> GetWithEmployeeAsync(Guid userId, CancellationToken ct = default)
        => await _set.Include(u => u.Employee).ThenInclude(e => e!.SalaryStructure)
                     .FirstOrDefaultAsync(u => u.Id == userId, ct);
}
