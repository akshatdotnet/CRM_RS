using Microsoft.AspNetCore.Mvc;
using UserHub.Application.DTOs;
using UserHub.Application.Interfaces;
using UserHub.Web.Filters;

namespace UserHub.Web.Controllers;

[RequireLogin]
[RequirePermission("Modules", "List")]
public class ModulesController : Controller
{
    private readonly IModuleService _moduleService;

    public ModulesController(IModuleService moduleService) => _moduleService = moduleService;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var modules = await _moduleService.GetAllModulesAsync();
        return View(modules);
    }

    [HttpGet]
    [RequirePermission("Modules", "Create")]
    public IActionResult Create() => View(new ModuleDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Modules", "Create")]
    public async Task<IActionResult> Create(ModuleDto model)
    {
        if (!ModelState.IsValid) return View(model);
        var (success, error) = await _moduleService.CreateModuleAsync(model);
        if (!success) { ModelState.AddModelError(string.Empty, error!); return View(model); }
        TempData["Success"] = "Module created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [RequirePermission("Modules", "Edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var m = await _moduleService.GetModuleByIdAsync(id);
        if (m == null) return NotFound();
        return View(m);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Modules", "Edit")]
    public async Task<IActionResult> Edit(ModuleDto model)
    {
        if (!ModelState.IsValid) return View(model);
        var (success, error) = await _moduleService.UpdateModuleAsync(model);
        if (!success) { ModelState.AddModelError(string.Empty, error!); return View(model); }
        TempData["Success"] = "Module updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Modules", "Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (success, error) = await _moduleService.DeleteModuleAsync(id);
        TempData[success ? "Success" : "Error"] = success ? "Module deleted." : error;
        return RedirectToAction(nameof(Index));
    }
}
