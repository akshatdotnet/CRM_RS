using Microsoft.EntityFrameworkCore;
using PersonalBrand.API.Data;
using PersonalBrand.API.Models.Entities;
using PersonalBrand.API.Repositories.Interfaces;
using PersonalBrand.Shared.Models;

namespace PersonalBrand.API.Repositories.Implementations;

// ─── Base Repository ──────────────────────────────────
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _ctx;
    protected readonly DbSet<T> _set;

    public Repository(AppDbContext ctx)
    {
        _ctx = ctx;
        _set = ctx.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync() =>
        await _set.AsNoTracking().OrderBy(e => e.Id).ToListAsync();

    public virtual async Task<T?> GetByIdAsync(int id) =>
        await _set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

    public virtual async Task<T> AddAsync(T entity)
    {
        await _set.AddAsync(entity);
        await _ctx.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _set.Update(entity);
        await _ctx.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await _set.FindAsync(id);
        if (entity != null) { entity.IsDeleted = true; await _ctx.SaveChangesAsync(); }
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _set.AnyAsync(e => e.Id == id);
}

// ─── Persona ──────────────────────────────────────────
public class PersonaRepository : Repository<Persona>, IPersonaRepository
{
    public PersonaRepository(AppDbContext ctx) : base(ctx) { }
    public async Task<Persona?> GetFirstAsync() =>
        await _set.AsNoTracking().FirstOrDefaultAsync();
}

// ─── Lead ─────────────────────────────────────────────
public class LeadRepository : Repository<Lead>, ILeadRepository
{
    public LeadRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<IEnumerable<Lead>> GetAllWithNotesAsync() =>
        await _set.AsNoTracking()
            .Include(l => l.Notes.OrderByDescending(n => n.CreatedAt))
            .Include(l => l.StatusHistory.OrderByDescending(h => h.CreatedAt))
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

    public async Task<Lead?> GetWithNotesAsync(int id) =>
        await _set.AsNoTracking()
            .Include(l => l.Notes.OrderByDescending(n => n.CreatedAt))
            .Include(l => l.StatusHistory.OrderByDescending(h => h.CreatedAt))
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<IEnumerable<Lead>> GetByStatusAsync(string status) =>
        await _set.AsNoTracking()
            .Where(l => l.Status == status)
            .Include(l => l.Notes)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

    public async Task<Lead?> GetByEmailAsync(string email) =>
        await _set.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Email.ToLower() == email.ToLower());

    public async Task<Dictionary<string, int>> GetCountByStatusAsync()
    {
        return await _set.GroupBy(l => l.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task AddNoteAsync(int leadId, string note, string addedBy = "Owner")
    {
        var leadNote = new LeadNote
        {
            LeadId = leadId,
            Note = note,
            AddedBy = addedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _ctx.LeadNotes.AddAsync(leadNote);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(int leadId, string newStatus, string changedBy = "Owner")
    {
        var lead = await _set.FindAsync(leadId);
        if (lead == null) return;

        var history = new LeadStatusHistory
        {
            LeadId = leadId,
            FromStatus = lead.Status,
            ToStatus = newStatus,
            ChangedBy = changedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _ctx.LeadStatusHistories.AddAsync(history);
        lead.Status = newStatus;
        lead.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
    }

    public async Task<int> GetTotalCountAsync() => await _set.CountAsync();
}

// ─── Q&A ──────────────────────────────────────────────
public class QARepository : Repository<QAItem>, IQARepository
{
    public QARepository(AppDbContext ctx) : base(ctx) { }

    public async Task<IEnumerable<QAItem>> GetByLevelAsync(string level) =>
        await _set.AsNoTracking()
            .Where(q => q.Level == level && q.IsActive)
            .OrderBy(q => q.SortOrder).ToListAsync();

    public async Task<IEnumerable<QAItem>> SearchAsync(string query) =>
        await _set.AsNoTracking()
            .Where(q => q.IsActive &&
                (EF.Functions.Like(q.Question, $"%{query}%") ||
                 EF.Functions.Like(q.Answer, $"%{query}%") ||
                 EF.Functions.Like(q.Category, $"%{query}%")))
            .OrderBy(q => q.SortOrder).ToListAsync();

    public async Task<IEnumerable<QAItem>> GetByCategoryAsync(string category) =>
        await _set.AsNoTracking()
            .Where(q => q.Category == category && q.IsActive)
            .OrderBy(q => q.SortOrder).ToListAsync();
}

// ─── Blog ─────────────────────────────────────────────
public class BlogRepository : Repository<BlogPost>, IBlogRepository
{
    public BlogRepository(AppDbContext ctx) : base(ctx) { }

    public async Task<BlogPost?> GetBySlugAsync(string slug) =>
        await _set.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Slug == slug && b.IsPublished);

    public async Task<IEnumerable<BlogPost>> GetPublishedAsync() =>
        await _set.AsNoTracking()
            .Where(b => b.IsPublished)
            .OrderByDescending(b => b.PublishedDate).ToListAsync();

    public async Task<IEnumerable<BlogPost>> GetByCategoryAsync(string category) =>
        await _set.AsNoTracking()
            .Where(b => b.Category == category && b.IsPublished)
            .OrderByDescending(b => b.PublishedDate).ToListAsync();

    public async Task IncrementViewCountAsync(int id)
    {
        var post = await _set.FindAsync(id);
        if (post != null) { post.ViewCount++; await _ctx.SaveChangesAsync(); }
    }

    public async Task<PagedResponse<BlogPost>> GetPagedAsync(int page, int pageSize)
    {
        var query = _set.AsNoTracking().Where(b => b.IsPublished);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.PublishedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return new PagedResponse<BlogPost> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }
}

// ─── Subscriber ───────────────────────────────────────
public class SubscriberRepository : ISubscriberRepository
{
    private readonly AppDbContext _ctx;
    public SubscriberRepository(AppDbContext ctx) { _ctx = ctx; }

    public async Task<bool> ExistsAsync(string email) =>
        await _ctx.Subscribers.AnyAsync(s => s.Email.ToLower() == email.ToLower());

    public async Task AddAsync(string email, string source = "footer")
    {
        await _ctx.Subscribers.AddAsync(new Subscriber
        {
            Email = email,
            Source = source,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _ctx.SaveChangesAsync();
    }

    public async Task<int> GetTotalCountAsync() =>
        await _ctx.Subscribers.CountAsync(s => s.IsActive);
}
