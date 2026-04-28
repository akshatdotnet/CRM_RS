using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Web.Controllers;

/// <summary>
/// Public-facing controller — no authentication required.
/// Routes: /p/{slug}  (property detail + enquiry)
///         /p/{slug}/confirm  (deal confirmation form)
///         /p/confirm/success  (thank you page)
///         /p/confirm/{token}  (admin: view submitted confirmation)
/// </summary>
[Route("p")]
public class PublicPropertyController : Controller
{
    private readonly IPropertyRepository _properties;
    private readonly IPropertyEnquiryRepository _enquiries;
    private readonly IDealConfirmationRepository _confirmations;
    private readonly IAgentRepository _agents;

    public PublicPropertyController(IPropertyRepository properties, IPropertyEnquiryRepository enquiries,
        IDealConfirmationRepository confirmations, IAgentRepository agents)
    {
        _properties = properties; _enquiries = enquiries; _confirmations = confirmations; _agents = agents;
    }

    // GET /p/{slug}  — public property page
    [HttpGet("{slug}")]
    public async Task<IActionResult> View(string slug)
    {
        var p = await _properties.GetBySlugAsync(slug);
        if (p == null) return NotFound();

        var vm = new PublicPropertyViewModel
        {
            Id = p.Id, Title = p.Title, PropertyType = p.PropertyType,
            Location = p.Location, Address = p.Address, City = p.City,
            PriceLabel = p.PriceLabel, Price = p.Price, Description = p.Description,
            AreaSqFt = p.AreaSqFt, Bedrooms = p.Bedrooms, Bathrooms = p.Bathrooms,
            Floors = p.Floors, YearBuilt = p.YearBuilt, IsFurnished = p.IsFurnished,
            HasParking = p.HasParking, HasGym = p.HasGym, HasPool = p.HasPool,
            HasSecurity = p.HasSecurity, Status = p.Status, Slug = slug,
            AgentName = p.CreatedBy?.FullName ?? "",
            AgentPhone = p.CreatedBy?.Phone ?? "",
            Amenities = p.Amenities.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList(),
            Photos = p.Photos.OrderBy(ph => ph.SortOrder).Select(ph => new PhotoItem { Id = ph.Id, FilePath = ph.FilePath, Caption = ph.Caption, IsPrimary = ph.IsPrimary }).ToList()
        };
        return View(vm);
    }

    // POST /p/{slug}/enquire — client submits quick interest form
    [HttpPost("{slug}/enquire"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Enquire(string slug, ClientEnquiryFormViewModel vm)
    {
        var p = await _properties.GetBySlugAsync(slug);
        if (p == null) return NotFound();
        if (!ModelState.IsValid) return RedirectToAction(nameof(View), new { slug });

        await _enquiries.CreateAsync(new PropertyEnquiry
        {
            PropertyId = p.Id,
            ClientName = vm.ClientName,
            ClientPhone = vm.ClientPhone,
            ClientEmail = vm.ClientEmail,
            Message = vm.Message,
            IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
        });
        TempData["EnquirySuccess"] = "Thank you! An agent will contact you shortly.";
        return RedirectToAction(nameof(View), new { slug });
    }

    // GET /p/{slug}/confirm — deal confirmation form (client intent to buy)
    [HttpGet("{slug}/confirm")]
    public async Task<IActionResult> Confirm(string slug)
    {
        var p = await _properties.GetBySlugAsync(slug);
        if (p == null || p.Status != "Available") return NotFound();
        var primaryPhoto = p.Photos.FirstOrDefault(ph => ph.IsPrimary)?.FilePath ?? p.Photos.FirstOrDefault()?.FilePath;
        return View(new ClientDealConfirmationViewModel
        {
            PropertyId = p.Id,
            PropertyTitle = p.Title,
            PropertyLocation = $"{p.Location}, {p.City}",
            PriceLabel = p.PriceLabel,
            PrimaryPhoto = primaryPhoto,
            OfferedPrice = p.Price
        });
    }

    // POST /p/{slug}/confirm — submit deal confirmation
    [HttpPost("{slug}/confirm"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(string slug, ClientDealConfirmationViewModel vm)
    {
        var p = await _properties.GetBySlugAsync(slug);
        if (p == null) return NotFound();
        if (!ModelState.IsValid) return View(vm);

        var confirmation = await _confirmations.CreateAsync(new DealConfirmation
        {
            PropertyId = p.Id,
            ClientName = vm.ClientName, ClientPhone = vm.ClientPhone, ClientEmail = vm.ClientEmail,
            ClientPAN = vm.ClientPAN, ClientAadhaar = vm.ClientAadhaar,
            OfferedPrice = vm.OfferedPrice, PaymentMode = vm.PaymentMode,
            LoanBank = vm.LoanBank, Notes = vm.Notes, ClientConsent = vm.ClientConsent
        });

        return RedirectToAction(nameof(ConfirmSuccess), new
        {
            token = confirmation.ConfirmationToken,
            name = vm.ClientName,
            title = p.Title,
            agent = p.CreatedBy?.Phone ?? ""
        });
    }

    // GET /p/confirm/success
    [HttpGet("confirm/success")]
    public IActionResult ConfirmSuccess(string token, string name, string title, string agent)
    {
        return View(new ConfirmationSuccessViewModel
        {
            ConfirmationToken = token,
            ClientName = name,
            PropertyTitle = title,
            AgentPhone = agent
        });
    }
}
