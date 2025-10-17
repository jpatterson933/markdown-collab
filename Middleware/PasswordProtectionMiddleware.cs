using MarkdownCollab.Common;

namespace MarkdownCollab.Middleware;

public class PasswordProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _expectedPassword;

    public PasswordProtectionMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _expectedPassword = RetrieveConfiguredPassword(configuration);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldAllowAccessWithoutAuthentication(context))
        {
            await _next(context);
            return;
        }

        if (UserIsAuthenticated(context))
        {
            await _next(context);
            return;
        }

        RedirectToLoginPage(context);
    }

    private static bool ShouldAllowAccessWithoutAuthentication(HttpContext context)
    {
        return IsPublicPath(context.Request.Path);
    }

    private static bool IsPublicPath(PathString requestPath)
    {
        return requestPath.StartsWithSegments(ApplicationConstants.Routes.Login) ||
               requestPath.StartsWithSegments(ApplicationConstants.Routes.Authenticate) ||
               requestPath.StartsWithSegments(ApplicationConstants.Routes.Logout);
    }

    private static bool UserIsAuthenticated(HttpContext context)
    {
        var authenticationStatus = context.Session.GetString(ApplicationConstants.Authentication.SessionKey);
        return authenticationStatus == ApplicationConstants.Authentication.SessionValue;
    }

    private static void RedirectToLoginPage(HttpContext context)
    {
        context.Response.Redirect(ApplicationConstants.Routes.Login);
    }

    private static string RetrieveConfiguredPassword(IConfiguration configuration)
    {
        var password = configuration[ApplicationConstants.Authentication.PasswordConfigKey]
            ?? Environment.GetEnvironmentVariable(ApplicationConstants.Authentication.PasswordConfigKey);

        if (string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException(ApplicationConstants.ErrorMessages.PasswordNotConfigured);
        }

        return password;
    }
}

public static class PasswordProtectionMiddlewareExtensions
{
    public static IApplicationBuilder UsePasswordProtection(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PasswordProtectionMiddleware>();
    }
}
