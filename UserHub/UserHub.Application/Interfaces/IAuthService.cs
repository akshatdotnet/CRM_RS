using UserHub.Application.DTOs;

namespace UserHub.Application.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string? Error, SessionUserDto? User)> LoginAsync(string username, string password);
    Task<SessionUserDto?> GetSessionUserAsync(Guid userId);
}
