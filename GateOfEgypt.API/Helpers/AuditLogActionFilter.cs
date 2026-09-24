using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using GateOfEgypt.Domain.Interfaces;
using GateOfEgypt.Domain.Models;

namespace GateOfEgypt.API.Helpers
{
    // Records who did what and when for every mutating (non-GET) Dashboard action.
    // Applied to DashboardController as a class-level [TypeFilter], so it never touches the public WebsiteController.
    public class AuditLogActionFilter : IAsyncActionFilter
    {
        private readonly IGenericRepository<AuditLog> _auditLogRepo;
        private readonly ILogger<AuditLogActionFilter> _logger;

        public AuditLogActionFilter(IGenericRepository<AuditLog> auditLogRepo, ILogger<AuditLogActionFilter> logger)
        {
            _auditLogRepo = auditLogRepo;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpMethod = context.HttpContext.Request.Method;
            var actionArguments = context.ActionArguments;

            var executedContext = await next();

            // Reads aren't audited, only actions that actually change something.
            if (HttpMethods.IsGet(httpMethod))
                return;

            var actionName = context.ActionDescriptor.RouteValues.TryGetValue("action", out var name)
                ? name
                : context.ActionDescriptor.DisplayName;
            var controllerName = context.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller)
                ? controller
                : "Dashboard";

            var statusCode = GetStatusCode(executedContext);

            var userEmail = executedContext.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(userEmail))
                userEmail = FindEmailInArguments(actionArguments) ?? "Anonymous";

            var entityId = actionArguments.TryGetValue("id", out var idValue) ? idValue?.ToString() : null;

            var auditLog = new AuditLog
            {
                UserId = executedContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                UserEmail = userEmail,
                HttpMethod = httpMethod,
                Action = ToReadableName(actionName ?? "Unknown"),
                Controller = controllerName ?? "Dashboard",
                EntityId = entityId,
                StatusCode = statusCode,
                IsSuccess = statusCode is >= 200 and < 300,
                Timestamp = DateTime.Now
            };

            try
            {
                await _auditLogRepo.AddAsync(auditLog);
                await _auditLogRepo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Never let audit logging break the actual request.
                _logger.LogError(ex, "Failed to write audit log for action {Action}", actionName);
            }
        }

        private static int GetStatusCode(ActionExecutedContext context)
        {
            return context.Result switch
            {
                ObjectResult { StatusCode: not null } objectResult => objectResult.StatusCode.Value,
                StatusCodeResult statusCodeResult => statusCodeResult.StatusCode,
                _ => context.HttpContext.Response.StatusCode is > 0 ? context.HttpContext.Response.StatusCode : StatusCodes.Status200OK
            };
        }

        // Fallback for pre-auth actions (login, forgot/reset password) where there's no authenticated
        // user yet: pulls an "Email" property off whichever DTO argument the action bound.
        private static string? FindEmailInArguments(IDictionary<string, object?> actionArguments)
        {
            foreach (var arg in actionArguments.Values)
            {
                if (arg == null)
                    continue;

                var emailProp = arg.GetType().GetProperty("Email");
                if (emailProp?.GetValue(arg) is string email && !string.IsNullOrWhiteSpace(email))
                    return email;
            }

            return null;
        }

        private static string ToReadableName(string input) =>
            Regex.Replace(input, "(\\B[A-Z])", " $1");
    }
}
