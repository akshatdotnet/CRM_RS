using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UserHub.Application.DTOs;
using UserHub.Shared.Constants;
using UserHub.Shared.Extensions;

namespace UserHub.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireLoginAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var user = session.GetObject<SessionUserDto>(SessionKeys.SessionUser);

        if (user == null)
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Auth",
                new { returnUrl = returnUrl.ToString() });
            return;
        }

        // Attach to HttpContext.Items for downstream access
        context.HttpContext.Items["SessionUser"] = user;
        base.OnActionExecuting(context);
    }
}
