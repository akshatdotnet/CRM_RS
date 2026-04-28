using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Web.Controllers;

[Authorize]
public class PropertyController : Controller
{
    private readonly IPropertyRepository _properties;
    private readonly IPropertyEnquiryRepository _enquiries;
    private readonly IDealConfirmationRepository _confirmations;
    private readonly IAgentRepository _agents;
    private readonly IWebHostEnvironment _env;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    public PropertyController(IPropertyRepository properties, IPropertyEnquiryRepository enquiries,
        IDealConfirmationRepository confirmations, IAgentRepository agents, IWebHostEnvironment env)
    {
        _properties = properties; _enquiries = enquiries;
        _confirmations = confirmations; _agents = agents; _env = env;
    }

    public async Task<IActionResult> Index(string? status, string? q)
    {
        var all = string.IsNullOrEmpty(q)
            ? string.IsNullOrEmpty(status) ? await _properties.GetAllAsync() : await _properties.GetByStatusAsync(status)
            : await _properties.SearchAsync(q);

        var allList = (await _properties.GetAllAsync()).ToList();
        var vm = new PropertyListViewModel
        {
            StatusFilter = status, SearchQuery = q,
            TotalAvailable = allList.Count(p => p.Status == "Available"),
            TotalSold = allList.Count(p => p.Status == "Sold"),
            TotalOnHold = allList.Count(p => p.Status == "Hold"),
            PendingEnquiries = await _enquiries.GetNewCountAsync(),
            PendingConfirmations = await _confirmations.GetPendingCountAsync(),
            Properties = all.Select(p => new PropertyCardItem
            {
                Id = p.Id, Title = p.Title, PropertyType = p.PropertyType,
                Location = p.Location, City = p.City, Price = p.Price,
                PriceLabel = p.PriceLabel, AreaSqFt = p.AreaSqFt,
                Bedrooms = p.Bedrooms, Bathrooms = p.Bathrooms,
                Status = p.Status, PublicSlug = p.PublicSlug,
                PrimaryPhotoPath = p.Photos.FirstOrDefault(ph => ph.IsPrimary)?.FilePath
                    ?? p.Photos.FirstOrDefault()?.FilePath,
                EnquiryCount = p.Enquiries?.Count ?? 0
            }).ToList()
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var agents = await _agents.GetAllAsync();
        return View(new PropertyFormViewModel
        {
            Agents = agents.Select(a => new AgentSelectItem { Id = a.Id, FullName = a.FullName }).ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)] // 50MB total
    public async Task<IActionResult> Create(PropertyFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Agents = (await _agents.GetAllAsync()).Select(a => new AgentSelectItem { Id = a.Id, FullName = a.FullName }).ToList();
            return View(vm);
        }
        var property = MapToEntity(vm, new Property());
        await _properties.CreateAsync(property);
        await SavePhotosAsync(vm.Photos, property.Id, isPrimary: true);
        TempData["Success"] = $"Property '{property.Title}' created. Share URL: /p/{property.PublicSlug}";
        return RedirectToAction(nameof(Details), new { id = property.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var p = await _properties.GetByIdAsync(id);
        if (p == null) return NotFound();
        var agents = await _agents.GetAllAsync();
        return View(new PropertyFormViewModel
        {
            Id = p.Id, Title = p.Title, PropertyType = p.PropertyType,
            Location = p.Location, Address = p.Address, City = p.City,
            Price = p.Price, PriceLabel = p.PriceLabel, Description = p.Description,
            AreaSqFt = p.AreaSqFt, Bedrooms = p.Bedrooms, Bathrooms = p.Bathrooms,
            Floors = p.Floors, YearBuilt = p.YearBuilt, IsFurnished = p.IsFurnished,
            HasParking = p.HasParking, HasGym = p.HasGym, HasPool = p.HasPool,
            HasSecurity = p.HasSecurity, Amenities = p.Amenities,
            Status = p.Status, IsPublished = p.IsPublished, CreatedByAgentId = p.CreatedByAgentId,
            Agents = agents.Select(a => new AgentSelectItem { Id = a.Id, FullName = a.FullName }).ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Edit(int id, PropertyFormViewModel vm)
    {
        var property = await _properties.GetByIdAsync(id);
        if (property == null) return NotFound();
        if (!ModelState.IsValid)
        {
            vm.Agents = (await _agents.GetAllAsync()).Select(a => new AgentSelectItem { Id = a.Id, FullName = a.FullName }).ToList();
            return View(vm);
        }
        MapToEntity(vm, property);
        await _properties.UpdateAsync(property);
        if (vm.Photos.Any())
            await SavePhotosAsync(vm.Photos, property.Id, isPrimary: false);
        TempData["Success"] = "Property updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var p = await _properties.GetByIdAsync(id);
        if (p == null) return NotFound();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var vm = new PropertyDetailViewModel
        {
            Id = p.Id, Title = p.Title, PropertyType = p.PropertyType,
            Location = p.Location, Address = p.Address, City = p.City,
            Price = p.Price, PriceLabel = p.PriceLabel, Description = p.Description,
            AreaSqFt = p.AreaSqFt, Bedrooms = p.Bedrooms, Bathrooms = p.Bathrooms,
            Floors = p.Floors, YearBuilt = p.YearBuilt, IsFurnished = p.IsFurnished,
            HasParking = p.HasParking, HasGym = p.HasGym, HasPool = p.HasPool,
            HasSecurity = p.HasSecurity, Status = p.Status, IsPublished = p.IsPublished,
            PublicSlug = p.PublicSlug, CreatedByAgent = p.CreatedBy?.FullName ?? "",
            CreatedAt = p.CreatedAt, ShareableUrl = $"{baseUrl}/p/{p.PublicSlug}",
            Amenities = p.Amenities.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList(),
            Photos = p.Photos.Select(ph => new PhotoItem { Id = ph.Id, FilePath = ph.FilePath, Caption = ph.Caption, IsPrimary = ph.IsPrimary }).ToList(),
            Enquiries = p.Enquiries.Select(e => new EnquiryItem { Id = e.Id, ClientName = e.ClientName, ClientPhone = e.ClientPhone, ClientEmail = e.ClientEmail, Message = e.Message, Status = e.Status, CreatedAt = e.CreatedAt }).ToList(),
            Confirmations = p.DealConfirmations.Select(d => new ConfirmationItem { Id = d.Id, ClientName = d.ClientName, ClientPhone = d.ClientPhone, ClientEmail = d.ClientEmail, OfferedPrice = d.OfferedPrice, PaymentMode = d.PaymentMode, Status = d.Status, SubmittedAt = d.SubmittedAt, ConfirmationToken = d.ConfirmationToken }).ToList()
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhoto(int photoId, int propertyId)
    {
        var p = await _properties.GetByIdAsync(propertyId);
        var photo = p?.Photos.FirstOrDefault(ph => ph.Id == photoId);
        if (photo != null)
        {
            var fullPath = Path.Combine(_env.WebRootPath, photo.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            await _properties.DeletePhotoAsync(photoId);
        }
        return RedirectToAction(nameof(Details), new { id = propertyId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPrimaryPhoto(int photoId, int propertyId)
    {
        await _properties.SetPrimaryPhotoAsync(propertyId, photoId);
        return RedirectToAction(nameof(Details), new { id = propertyId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _properties.DeleteAsync(id);
        TempData["Success"] = "Property deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateEnquiryStatus(int enquiryId, string status, int propertyId)
    {
        await _enquiries.UpdateStatusAsync(enquiryId, status);
        return RedirectToAction(nameof(Details), new { id = propertyId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewConfirmation(int confirmationId, string status, int propertyId)
    {
        await _confirmations.UpdateStatusAsync(confirmationId, status, agentId: 1); // Use auth agent in production
        if (status == "Approved")
        {
            // Auto update property status to Sold
            var prop = await _properties.GetByIdAsync(propertyId);
            if (prop != null) { prop.Status = "Sold"; await _properties.UpdateAsync(prop); }
        }
        TempData["Success"] = $"Deal confirmation {status}.";
        return RedirectToAction(nameof(Details), new { id = propertyId });
    }

    // ── Helpers ────────────────────────────────────────────────────────
    private Property MapToEntity(PropertyFormViewModel vm, Property p)
    {
        p.Title = vm.Title; p.PropertyType = vm.PropertyType; p.Description = vm.Description;
        p.Location = vm.Location; p.Address = vm.Address; p.City = vm.City;
        p.Price = vm.Price; p.PriceLabel = vm.PriceLabel;
        p.AreaSqFt = vm.AreaSqFt; p.Bedrooms = vm.Bedrooms; p.Bathrooms = vm.Bathrooms;
        p.Floors = vm.Floors; p.YearBuilt = vm.YearBuilt; p.IsFurnished = vm.IsFurnished;
        p.HasParking = vm.HasParking; p.HasGym = vm.HasGym; p.HasPool = vm.HasPool;
        p.HasSecurity = vm.HasSecurity; p.Amenities = vm.Amenities;
        p.Status = vm.Status; p.IsPublished = vm.IsPublished; p.CreatedByAgentId = vm.CreatedByAgentId > 0 ? vm.CreatedByAgentId : 1;
        return p;
    }

    private async Task SavePhotosAsync(List<IFormFile> files, int propertyId, bool isPrimary)
    {
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "properties", propertyId.ToString());
        Directory.CreateDirectory(uploadDir);
        int order = 0;
        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(ext) || file.Length > MaxFileSize) continue;
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadDir, fileName);
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            await _properties.AddPhotoAsync(new PropertyPhoto
            {
                PropertyId = propertyId,
                FileName = file.FileName,
                FilePath = $"/uploads/properties/{propertyId}/{fileName}",
                IsPrimary = isPrimary && order == 0,
                SortOrder = order++
            });
        }
    }
}
