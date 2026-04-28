using PersonalBrand.API.Models.Entities;
using PersonalBrand.Shared.Models;

namespace PersonalBrand.API.Repositories.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);  // Soft delete
    Task<bool> ExistsAsync(int id);
}

public interface IPersonaRepository : IRepository<Persona>
{
    Task<Persona?> GetFirstAsync();
}

public interface ILeadRepository : IRepository<Lead>
{
    Task<IEnumerable<Lead>> GetByStatusAsync(string status);
    Task<IEnumerable<Lead>> GetAllWithNotesAsync();
    Task<Lead?> GetWithNotesAsync(int id);
    Task<Lead?> GetByEmailAsync(string email);
    Task<Dictionary<string, int>> GetCountByStatusAsync();
    Task AddNoteAsync(int leadId, string note, string addedBy = "Owner");
    Task UpdateStatusAsync(int leadId, string newStatus, string changedBy = "Owner");
    Task<int> GetTotalCountAsync();
}

public interface IQARepository : IRepository<QAItem>
{
    Task<IEnumerable<QAItem>> GetByLevelAsync(string level);
    Task<IEnumerable<QAItem>> SearchAsync(string query);
    Task<IEnumerable<QAItem>> GetByCategoryAsync(string category);
}

public interface IBlogRepository : IRepository<BlogPost>
{
    Task<BlogPost?> GetBySlugAsync(string slug);
    Task<IEnumerable<BlogPost>> GetPublishedAsync();
    Task<IEnumerable<BlogPost>> GetByCategoryAsync(string category);
    Task IncrementViewCountAsync(int id);
    Task<PagedResponse<BlogPost>> GetPagedAsync(int page, int pageSize);
}

public interface ISubscriberRepository
{
    Task<bool> ExistsAsync(string email);
    Task AddAsync(string email, string source = "footer");
    Task<int> GetTotalCountAsync();
}
