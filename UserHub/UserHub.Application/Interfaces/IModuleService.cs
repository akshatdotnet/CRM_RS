using UserHub.Application.DTOs;

namespace UserHub.Application.Interfaces;

public interface IModuleService
{
    Task<IEnumerable<ModuleDto>> GetAllModulesAsync();
    Task<ModuleDto?> GetModuleByIdAsync(Guid id);
    Task<(bool Success, string? Error)> CreateModuleAsync(ModuleDto dto);
    Task<(bool Success, string? Error)> UpdateModuleAsync(ModuleDto dto);
    Task<(bool Success, string? Error)> DeleteModuleAsync(Guid id);
    Task<IEnumerable<ModuleDto>> GetAccessibleModulesAsync(SessionUserDto sessionUser);
}
