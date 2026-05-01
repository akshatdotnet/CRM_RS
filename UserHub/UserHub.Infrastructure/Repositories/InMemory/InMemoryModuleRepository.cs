using UserHub.Domain.Entities;
using UserHub.Domain.Interfaces;

namespace UserHub.Infrastructure.Repositories.InMemory;

public class InMemoryModuleRepository : IModuleRepository
{
    private readonly List<Module> _modules = new();

    public void Seed(List<Module> modules) => _modules.AddRange(modules);

    public Task<Module?> GetByIdAsync(Guid id) =>
        Task.FromResult(_modules.FirstOrDefault(m => m.Id == id));

    public Task<IEnumerable<Module>> GetAllAsync(bool includeInactive = false) =>
        Task.FromResult(_modules
            .Where(m => includeInactive || m.IsActive)
            .OrderBy(m => m.SortOrder)
            .AsEnumerable());

    public Task AddAsync(Module module) { _modules.Add(module); return Task.CompletedTask; }

    public Task UpdateAsync(Module module)
    {
        var idx = _modules.FindIndex(m => m.Id == module.Id);
        if (idx >= 0) _modules[idx] = module;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _modules.RemoveAll(m => m.Id == id);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null) =>
        Task.FromResult(_modules.Any(m =>
            m.Name.ToLower() == name.ToLower() &&
            (excludeId == null || m.Id != excludeId)));
}
