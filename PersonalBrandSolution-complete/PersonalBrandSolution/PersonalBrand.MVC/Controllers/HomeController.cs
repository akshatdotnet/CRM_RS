using Microsoft.AspNetCore.Mvc;
using PersonalBrand.MVC.Services;
using PersonalBrand.MVC.ViewModels;
using PersonalBrand.Shared.Models;

namespace PersonalBrand.MVC.Controllers;

public class HomeController : Controller
{
    private readonly PersonalBrandApiClient _api;
    private readonly ILogger<HomeController> _logger;

    public HomeController(PersonalBrandApiClient api, ILogger<HomeController> logger)
    {
        _api = api;
        _logger = logger;
    }

    // ─── Main Page ────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        // Load all data in parallel for performance
        var tasks = new
        {
            Persona      = _api.GetPersonaAsync(),
            Skills       = _api.GetSkillsAsync(),
            Roadmap      = _api.GetRoadmapAsync(),
            Courses      = _api.GetCoursesAsync(),
            Projects     = _api.GetProjectsAsync(),
            QA           = _api.GetQAAsync(),
            Services     = _api.GetServicesAsync(),
            Pipeline     = _api.GetPipelineAsync(),
            Blog         = _api.GetBlogPostsAsync(),
            Testimonials = _api.GetTestimonialsAsync()
        };

        await Task.WhenAll(
            tasks.Persona, tasks.Skills, tasks.Roadmap, tasks.Courses,
            tasks.Projects, tasks.QA, tasks.Services, tasks.Pipeline,
            tasks.Blog, tasks.Testimonials
        );

        var vm = new HomeViewModel
        {
            Persona      = tasks.Persona.Result      ?? new(),
            Skills       = tasks.Skills.Result       ?? [],
            Roadmap      = tasks.Roadmap.Result      ?? [],
            Courses      = tasks.Courses.Result      ?? [],
            Projects     = tasks.Projects.Result     ?? [],
            QAItems      = tasks.QA.Result           ?? [],
            Services     = tasks.Services.Result     ?? [],
            Pipeline     = tasks.Pipeline.Result     ?? new(),
            BlogPosts    = tasks.Blog.Result?.Items  ?? [],
            Testimonials = tasks.Testimonials.Result ?? []
        };

        // SEO ViewBag
        ViewBag.Title = $"{vm.Persona.Name} · Senior .NET Core & Azure Cloud Architect";
        ViewBag.Description = vm.Persona.Tagline;

        return View(vm);
    }

    // ─── Contact Form POST ────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact([FromForm] ContactFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastType"] = "error";
            TempData["ToastMsg"] = "Please fill all required fields.";
            return RedirectToAction(nameof(Index), null, "contact");
        }

        var dto = new ContactFormDto
        {
            Name = form.Name, Email = form.Email,
            Service = form.Service, Budget = form.Budget,
            Message = form.Message
        };

        var (success, message, _) = await _api.SubmitContactFormAsync(dto);
        TempData["ToastType"] = success ? "success" : "error";
        TempData["ToastMsg"] = message;

        return RedirectToAction(nameof(Index), null, "contact");
    }

    // ─── Newsletter POST ──────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Newsletter([FromForm] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["ToastType"] = "error";
            TempData["ToastMsg"] = "Please enter a valid email.";
            return RedirectToAction(nameof(Index), null, "footer");
        }

        var (success, message) = await _api.SubscribeNewsletterAsync(email);
        TempData["ToastType"] = success ? "success" : "info";
        TempData["ToastMsg"] = message;
        return RedirectToAction(nameof(Index), null, "footer");
    }

    // ─── AJAX: Q&A Filter ────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> QAFilter(string? level = null, string? search = null)
    {
        var items = await _api.GetQAAsync(level == "all" ? null : level, search);
        return Json(new { success = true, data = items ?? [] });
    }

    // ─── AJAX: Pipeline ──────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Pipeline()
    {
        var pipeline = await _api.GetPipelineAsync();
        return Json(new { success = true, data = pipeline });
    }

    // ─── AJAX: Update Lead Status ─────────────────────────
    [HttpPost]
    public async Task<IActionResult> UpdateLeadStatus([FromBody] UpdateLeadStatusRequest req)
    {
        var ok = await _api.UpdateLeadStatusAsync(req.LeadId, req.Status);
        return Json(new { success = ok, message = ok ? "Status updated" : "Update failed" });
    }

    // ─── AJAX: Add Lead Note ──────────────────────────────
    [HttpPost]
    public async Task<IActionResult> AddLeadNote([FromBody] AddLeadNoteRequest req)
    {
        var ok = await _api.AddLeadNoteAsync(req.LeadId, req.Note);
        return Json(new { success = ok, message = ok ? "Note added" : "Failed to add note" });
    }

    // ─── Error ────────────────────────────────────────────
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}

// ─── Request models for AJAX endpoints ───────────────
public record UpdateLeadStatusRequest(int LeadId, string Status);
public record AddLeadNoteRequest(int LeadId, string Note);
