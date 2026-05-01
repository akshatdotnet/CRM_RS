using Microsoft.AspNetCore.Mvc;
using UserHub.Application.DTOs;
using UserHub.Application.Interfaces;
using UserHub.Web.Filters;

namespace UserHub.Web.Controllers;

[RequireLogin]
[RequirePermission("Roles", "List")]
public class RolesController : Controller
{
    private readonly IRoleService _roleService;
    private readonly IModuleService _moduleService;

    public RolesController(IRoleService roleService, IModuleService moduleService)
    {
        _roleService = roleService;
        _moduleService = moduleService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return View(roles);
    }

    [HttpGet]
    [RequirePermission("Roles", "Create")]
    public IActionResult Create() => View(new CreateRoleDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Roles", "Create")]
    public async Task<IActionResult> Create(CreateRoleDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, error) = await _roleService.CreateRoleAsync(model);
        if (!success) { ModelState.AddModelError(string.Empty, error!); return View(model); }

        TempData["Success"] = "Role created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [RequirePermission("Roles", "Edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null) return NotFound();
        return View(new EditRoleDto { Id = role.Id, Name = role.Name, Description = role.Description });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Roles", "Edit")]
    public async Task<IActionResult> Edit(EditRoleDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, error) = await _roleService.UpdateRoleAsync(model);
        if (!success) { ModelState.AddModelError(string.Empty, error!); return View(model); }

        TempData["Success"] = "Role updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Roles", "Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (success, error) = await _roleService.DeleteRoleAsync(id);
        TempData[success ? "Success" : "Error"] = success ? "Role removed." : error;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [RequirePermission("Roles", "Edit")]
    public async Task<IActionResult> Permissions(Guid id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null) return NotFound();

        var perms = await _roleService.GetRolePermissionsAsync(id);
        ViewBag.Role = role;
        return View(perms);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Roles", "Edit")]
    public async Task<IActionResult> Permissions(Guid id, IEnumerable<PermissionSetDto> permissions)
    {
        var dto = new SetPermissionsDto { RoleId = id, Permissions = permissions };
        var (success, error) = await _roleService.SetPermissionsAsync(dto);
        TempData[success ? "Success" : "Error"] = success ? "Permissions saved." : error;
        return RedirectToAction(nameof(Index));
    }
}
