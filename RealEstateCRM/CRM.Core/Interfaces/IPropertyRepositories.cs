using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

public interface IPropertyRepository
{
    Task<IEnumerable<Property>> GetAllAsync();
    Task<Property?> GetByIdAsync(int id);
    Task<Property?> GetBySlugAsync(string slug);
    Task<IEnumerable<Property>> GetByStatusAsync(string status);
    Task<IEnumerable<Property>> SearchAsync(string query);
    Task<Property> CreateAsync(Property property);
    Task<Property> UpdateAsync(Property property);
    Task DeleteAsync(int id);
    Task<PropertyPhoto> AddPhotoAsync(PropertyPhoto photo);
    Task DeletePhotoAsync(int photoId);
    Task SetPrimaryPhotoAsync(int propertyId, int photoId);
}

public interface IPropertyEnquiryRepository
{
    Task<IEnumerable<PropertyEnquiry>> GetAllAsync();
    Task<IEnumerable<PropertyEnquiry>> GetByPropertyAsync(int propertyId);
    Task<PropertyEnquiry> CreateAsync(PropertyEnquiry enquiry);
    Task<PropertyEnquiry> UpdateStatusAsync(int id, string status);
    Task<int> GetNewCountAsync();
}

public interface IDealConfirmationRepository
{
    Task<IEnumerable<DealConfirmation>> GetAllAsync();
    Task<IEnumerable<DealConfirmation>> GetByPropertyAsync(int propertyId);
    Task<DealConfirmation?> GetByTokenAsync(string token);
    Task<DealConfirmation> CreateAsync(DealConfirmation confirmation);
    Task<DealConfirmation> UpdateStatusAsync(int id, string status, int agentId);
    Task<int> GetPendingCountAsync();
}
