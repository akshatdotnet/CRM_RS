using UserHub.Application.DTOs;
using UserHub.Application.Interfaces;
using UserHub.Domain.Entities;
using UserHub.Domain.Interfaces;

namespace UserHub.Application.Services;

public class ModuleService : IModuleService
{
    private readonly IModuleRepository _moduleRepo;

    public ModuleService(IModuleRepository moduleRepo) => _moduleRepo = moduleRepo;

    public async Task<IEnumerable<ModuleDto>> GetAllModulesAsync()
    {
        var modules = await _moduleRepo.GetAllAsync(includeInactive: true);
        return modules.OrderBy(m => m.SortOrder).Select(ToDto);
    }

    public async Task<ModuleDto?> GetModuleByIdAsync(Guid id)
    {
        var m = await _moduleRepo.GetByIdAsync(id);
        return m == null ? null : ToDto(m);
    }

    public async Task<(bool Success, string? Error)> CreateModuleAsync(ModuleDto dto)
    {
        if (await _moduleRepo.ExistsByNameAsync(dto.Name))
            return (false, "Module name already exists.");

        var module = Module.Create(dto.Name, dto.DisplayName, dto.ControllerName, dto.Icon, dto.SortOrder);
        await _moduleRepo.AddAsync(module);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateModuleAsync(ModuleDto dto)
    {
        var module = await _moduleRepo.GetByIdAsync(dto.Id);
        if (module == null) return (false, "Module not found.");

        if (await _moduleRepo.ExistsByNameAsync(dto.Name, dto.Id))
            return (false, "Module name already exists.");

        module.Update(dto.DisplayName, dto.Icon, dto.SortOrder);
        await _moduleRepo.UpdateAsync(module);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteModuleAsync(Guid id)
    {
        var module = await _moduleRepo.GetByIdAsync(id);
        if (module == null) return (false, "Module not found.");

        await _moduleRepo.DeleteAsync(id);
        return (true, null);
    }

    public async Task<IEnumerable<ModuleDto>> GetAccessibleModulesAsync(SessionUserDto sessionUser)
    {
        var modules = await _moduleRepo.GetAllAsync();
        return modules
            .Where(m => m.IsActive)
            .Where(m => sessionUser.IsSuperAdmin ||
                (sessionUser.Permissions.TryGetValue(m.Name, out var p) && p.CanView))
            .OrderBy(m => m.SortOrder)
            .Select(ToDto);
    }

    private static ModuleDto ToDto(Domain.Entities.Module m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        DisplayName = m.DisplayName,
        ControllerName = m.ControllerName,
        Icon = m.Icon,
        SortOrder = m.SortOrder,
        IsActive = m.IsActive
    };
}
