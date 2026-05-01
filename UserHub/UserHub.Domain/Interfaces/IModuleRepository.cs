using UserHub.Domain.Entities;

namespace UserHub.Domain.Interfaces;

public interface IModuleRepository
{
    Task<Module?> GetByIdAsync(Guid id);
    Task<IEnumerable<Module>> GetAllAsync(bool includeInactive = false);
    Task AddAsync(Module module);
    Task UpdateAsync(Module module);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
}
