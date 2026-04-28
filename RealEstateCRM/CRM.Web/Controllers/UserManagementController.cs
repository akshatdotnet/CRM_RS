using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IAgentRepository _agents;
    private readonly IEmailService _email;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IAgentRepository agents,
        IEmailService email)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _agents = agents;
        _email = email;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        var items = new List<UserRowItem>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            items.Add(new UserRowItem
            {
                Id = u.Id, FullName = u.FullName, Email = u.Email ?? "",
                Role = roles.FirstOrDefault() ?? "—", IsActive = u.IsActive,
                CreatedAt = u.CreatedAt, LastLoginAt = u.LastLoginAt,
                EmailConfirmed = u.EmailConfirmed
            });
        }
        return View(new UserListViewModel { Users = items });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var agents = await _agents.GetAllAsync();
        return View(new CreateUserViewModel
        {
            Agents = agents.Select(a => new AgentSelectItem { Id = a.Id, FullName = a.FullName }).ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Agents = (await _agents.GetAllAsync()).Select(a => new AgentSelectItem { Id = a.Id, FullName = a.FullName }).ToList();
            return View(vm);
        }

        // Generate a secure temporary password
        var tempPassword = $"Crm@{Guid.NewGuid().ToString()[..6].ToUpper()}!";

        var user = new ApplicationUser
        {
            FullName = vm.FullName,
            UserName = vm.Email,
            Email = vm.Email,
            EmailConfirmed = true,   // Admin-created users are pre-confirmed
            IsActive = true,
            AgentId = vm.AgentId > 0 ? vm.AgentId : null
        };

        var result = await _userManager.CreateAsync(user, tempPassword);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            vm.Agents = (await _agents.GetAllAsync()).Select(a => new AgentSelectItem { Id = a.Id, FullName = a.FullName }).ToList();
            return View(vm);
        }

        // Ensure role exists then assign
        if (!await _roleManager.RoleExistsAsync(vm.Role))
            await _roleManager.CreateAsync(new IdentityRole(vm.Role));
        await _userManager.AddToRoleAsync(user, vm.Role);

        // Send welcome email with temp password
        await _email.SendWelcomeEmailAsync(user.Email!, user.FullName, tempPassword);

        TempData["Success"] = $"User '{vm.FullName}' created. Temporary password sent to {vm.Email}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        var roles = await _userManager.GetRolesAsync(user);
        var agents = await _agents.GetAllAsync();
        return View(new EditUserViewModel
        {
            Id = user.Id, FullName = user.FullName, Email = user.Email ?? "",
            Role = roles.FirstOrDefault() ?? AppRoles.Agent,
            IsActive = user.IsActive, AgentId = user.AgentId,
            Agents = agents.Select(a => new AgentSelectItem { Id = a.Id, FullName = a.FullName }).ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditUserViewModel vm)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (!ModelState.IsValid)
        {
            vm.Agents = (await _agents.GetAllAsync()).Select(a => new AgentSelectItem { Id = a.Id, FullName = a.FullName }).ToList();
            return View(vm);
        }

        user.FullName = vm.FullName;
        user.Email = vm.Email;
        user.UserName = vm.Email;
        user.IsActive = vm.IsActive;
        user.AgentId = vm.AgentId > 0 ? vm.AgentId : null;
        await _userManager.UpdateAsync(user);

        // Update role
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!await _roleManager.RoleExistsAsync(vm.Role))
            await _roleManager.CreateAsync(new IdentityRole(vm.Role));
        await _userManager.AddToRoleAsync(user, vm.Role);

        TempData["Success"] = $"User '{vm.FullName}' updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetUserPassword(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = Url.Action("ResetPassword", "Account", new { userId = user.Id, token }, Request.Scheme)!;
        await _email.SendPasswordResetEmailAsync(user.Email!, user.FullName, resetLink);
        TempData["Success"] = $"Password reset link sent to {user.Email}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        // Prevent deactivating yourself
        var me = await _userManager.GetUserAsync(User);
        if (user.Id == me?.Id) { TempData["Error"] = "You cannot deactivate your own account."; return RedirectToAction(nameof(Index)); }
        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        TempData["Success"] = $"User {(user.IsActive ? "activated" : "deactivated")}.";
        return RedirectToAction(nameof(Index));
    }
}
