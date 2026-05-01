using Microsoft.AspNetCore.Mvc;
using UserHub.Application.DTOs;
using UserHub.Application.Interfaces;
using UserHub.Web.Filters;

namespace UserHub.Web.Controllers;

[RequireLogin]
[RequirePermission("Users", "List")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    public UsersController(IUserService userService, IRoleService roleService)
    {
        _userService = userService;
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(UserQueryDto query)
    {
        var result = await _userService.GetUsersAsync(query);
        ViewBag.Query = query;
        return View(result);
    }

    [HttpGet]
    [RequirePermission("Users", "View")]
    public async Task<IActionResult> Details(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpGet]
    [RequirePermission("Users", "Create")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await _roleService.GetAllRolesAsync();
        return View(new CreateUserDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Users", "Create")]
    public async Task<IActionResult> Create(CreateUserDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = await _roleService.GetAllRolesAsync();
            return View(model);
        }

        var (success, error) = await _userService.CreateUserAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            ViewBag.Roles = await _roleService.GetAllRolesAsync();
            return View(model);
        }

        TempData["Success"] = "User created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [RequirePermission("Users", "Edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        ViewBag.Roles = await _roleService.GetAllRolesAsync();
        return View(new EditUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            RoleIds = user.Roles.Select(r => r.Id)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Users", "Edit")]
    public async Task<IActionResult> Edit(EditUserDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = await _roleService.GetAllRolesAsync();
            return View(model);
        }

        var (success, error) = await _userService.UpdateUserAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            ViewBag.Roles = await _roleService.GetAllRolesAsync();
            return View(model);
        }

        TempData["Success"] = "User updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Users", "Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (success, error) = await _userService.DeleteUserAsync(id);
        TempData[success ? "Success" : "Error"] = success ? "User deleted." : error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Users", "Edit")]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var (success, error) = await _userService.ToggleActiveAsync(id);
        TempData[success ? "Success" : "Error"] = success ? "Status updated." : error;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [RequirePermission("Users", "Edit")]
    public async Task<IActionResult> ChangePassword(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return View(new ChangePasswordDto { UserId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("Users", "Edit")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, error) = await _userService.ChangePasswordAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(model);
        }

        TempData["Success"] = "Password changed successfully.";
        return RedirectToAction(nameof(Index));
    }
}
