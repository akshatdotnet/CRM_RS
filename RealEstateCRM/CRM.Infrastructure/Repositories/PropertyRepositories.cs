using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly CrmDbContext _db;
    public PropertyRepository(CrmDbContext db) => _db = db;

    public async Task<IEnumerable<Property>> GetAllAsync() =>
        await _db.Properties
            .Include(p => p.Photos.OrderBy(ph => ph.SortOrder))
            .Include(p => p.CreatedBy)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<Property?> GetByIdAsync(int id) =>
        await _db.Properties
            .Include(p => p.Photos.OrderBy(ph => ph.SortOrder))
            .Include(p => p.CreatedBy)
            .Include(p => p.Enquiries.OrderByDescending(e => e.CreatedAt))
            .Include(p => p.DealConfirmations.OrderByDescending(d => d.SubmittedAt))
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

    public async Task<Property?> GetBySlugAsync(string slug) =>
        await _db.Properties
            .Include(p => p.Photos.OrderBy(ph => ph.SortOrder))
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.PublicSlug == slug && p.IsPublished && p.IsActive);

    public async Task<IEnumerable<Property>> GetByStatusAsync(string status) =>
        await _db.Properties
            .Include(p => p.Photos.Where(ph => ph.IsPrimary))
            .Include(p => p.CreatedBy)
            .Where(p => p.Status == status && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Property>> SearchAsync(string query) =>
        await _db.Properties
            .Include(p => p.Photos.Where(ph => ph.IsPrimary))
            .Where(p => p.IsActive && (
                p.Title.Contains(query) ||
                p.Location.Contains(query) ||
                p.City.Contains(query) ||
                p.PropertyType.Contains(query)))
            .ToListAsync();

    public async Task<Property> CreateAsync(Property property)
    {
        // Generate unique slug from title
        var baseSlug = property.Title.ToLower()
            .Replace(" ", "-")
            .Replace(",", "")
            .Replace(".", "");
        property.PublicSlug = $"{baseSlug}-{Guid.NewGuid().ToString()[..6]}";
        _db.Properties.Add(property);
        await _db.SaveChangesAsync();
        return property;
    }

    public async Task<Property> UpdateAsync(Property property)
    {
        property.UpdatedAt = DateTime.UtcNow;
        _db.Properties.Update(property);
        await _db.SaveChangesAsync();
        return property;
    }

    public async Task DeleteAsync(int id)
    {
        var p = await _db.Properties.FindAsync(id);
        if (p != null) { p.IsActive = false; await _db.SaveChangesAsync(); }
    }

    public async Task<PropertyPhoto> AddPhotoAsync(PropertyPhoto photo)
    {
        _db.PropertyPhotos.Add(photo);
        await _db.SaveChangesAsync();
        return photo;
    }

    public async Task DeletePhotoAsync(int photoId)
    {
        var photo = await _db.PropertyPhotos.FindAsync(photoId);
        if (photo != null) { _db.PropertyPhotos.Remove(photo); await _db.SaveChangesAsync(); }
    }

    public async Task SetPrimaryPhotoAsync(int propertyId, int photoId)
    {
        var photos = await _db.PropertyPhotos.Where(p => p.PropertyId == propertyId).ToListAsync();
        photos.ForEach(p => p.IsPrimary = p.Id == photoId);
        await _db.SaveChangesAsync();
    }
}

public class PropertyEnquiryRepository : IPropertyEnquiryRepository
{
    private readonly CrmDbContext _db;
    public PropertyEnquiryRepository(CrmDbContext db) => _db = db;

    public async Task<IEnumerable<PropertyEnquiry>> GetAllAsync() =>
        await _db.PropertyEnquiries
            .Include(e => e.Property)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<PropertyEnquiry>> GetByPropertyAsync(int propertyId) =>
        await _db.PropertyEnquiries
            .Where(e => e.PropertyId == propertyId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

    public async Task<PropertyEnquiry> CreateAsync(PropertyEnquiry enquiry)
    {
        _db.PropertyEnquiries.Add(enquiry);
        await _db.SaveChangesAsync();
        return enquiry;
    }

    public async Task<PropertyEnquiry> UpdateStatusAsync(int id, string status)
    {
        var e = await _db.PropertyEnquiries.FindAsync(id) ?? throw new KeyNotFoundException();
        e.Status = status;
        await _db.SaveChangesAsync();
        return e;
    }

    public async Task<int> GetNewCountAsync() =>
        await _db.PropertyEnquiries.CountAsync(e => e.Status == "New");
}

public class DealConfirmationRepository : IDealConfirmationRepository
{
    private readonly CrmDbContext _db;
    public DealConfirmationRepository(CrmDbContext db) => _db = db;

    public async Task<IEnumerable<DealConfirmation>> GetAllAsync() =>
        await _db.DealConfirmations
            .Include(d => d.Property)
            .Include(d => d.ReviewedBy)
            .OrderByDescending(d => d.SubmittedAt)
            .ToListAsync();

    public async Task<IEnumerable<DealConfirmation>> GetByPropertyAsync(int propertyId) =>
        await _db.DealConfirmations
            .Where(d => d.PropertyId == propertyId)
            .OrderByDescending(d => d.SubmittedAt)
            .ToListAsync();

    public async Task<DealConfirmation?> GetByTokenAsync(string token) =>
        await _db.DealConfirmations
            .Include(d => d.Property).ThenInclude(p => p.Photos)
            .FirstOrDefaultAsync(d => d.ConfirmationToken == token);

    public async Task<DealConfirmation> CreateAsync(DealConfirmation confirmation)
    {
        confirmation.ConfirmationToken = Guid.NewGuid().ToString("N"); // 32-char hex token
        _db.DealConfirmations.Add(confirmation);
        await _db.SaveChangesAsync();
        return confirmation;
    }

    public async Task<DealConfirmation> UpdateStatusAsync(int id, string status, int agentId)
    {
        var d = await _db.DealConfirmations.FindAsync(id) ?? throw new KeyNotFoundException();
        d.Status = status;
        d.ReviewedAt = DateTime.UtcNow;
        d.ReviewedByAgentId = agentId;
        await _db.SaveChangesAsync();
        return d;
    }

    public async Task<int> GetPendingCountAsync() =>
        await _db.DealConfirmations.CountAsync(d => d.Status == "Pending");
}
