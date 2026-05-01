using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UserHub.Application.DTOs;
using UserHub.Shared.Constants;
using UserHub.Shared.Extensions;

namespace UserHub.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : ActionFilterAttribute
{
    private readonly string _module;
    private readonly string _permission; // "View","Create","Edit","Delete","List"

    public RequirePermissionAttribute(string module, string permission)
    {
        _module = module;
        _permission = permission;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.Items["SessionUser"] as SessionUserDto
            ?? context.HttpContext.Session.GetObject<SessionUserDto>(SessionKeys.SessionUser);

        if (user == null)
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        if (user.IsSuperAdmin) { base.OnActionExecuting(context); return; }

        if (!user.Permissions.TryGetValue(_module, out var perm))
        {
            context.Result = new ForbidResult();
            return;
        }

        var allowed = _permission switch
        {
            "View"   => perm.CanView,
            "Create" => perm.CanCreate,
            "Edit"   => perm.CanEdit,
            "Delete" => perm.CanDelete,
            "List"   => perm.CanList,
            _        => false
        };

        if (!allowed) context.Result = new ForbidResult();
        else base.OnActionExecuting(context);
    }
}
