using HRMS.Domain.Entities;
using System.Security.Claims;

namespace HRMS.Application.Interfaces.Services;

//public interface IJwtService
//{
//    string GenerateAccessToken(User user);
//    string GenerateRefreshToken();
//    Guid? ValidateTokenAndGetUserId(string token);
//}

public interface IJwtService
{
    (string token, DateTime expiry) GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}