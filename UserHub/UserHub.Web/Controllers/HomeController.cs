using Microsoft.AspNetCore.Mvc;
using UserHub.Application.Interfaces;
using UserHub.Web.Filters;

namespace UserHub.Web.Controllers;

[RequireLogin]
public class HomeController : Controller
{
    private readonly IUserService _userService;
    private readonly IModuleService _moduleService;

    public HomeController(IUserService userService, IModuleService moduleService)
    {
        _userService = userService;
        _moduleService = moduleService;
    }

    public async Task<IActionResult> Dashboard()
    {
        var stats = await _userService.GetDashboardStatsAsync();
        return View(stats);
    }
}
