using System.Security.Claims;
using GestorInventario.Application.Interfaces;
using GestorInventario.Domain.Usuarios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GestorInventario.Api.Authentication;

internal static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddApiAuthentication(
        this IServiceCollection services)
    {
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "gestor-inventario.session";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
                options.Events.OnValidatePrincipal = ValidatePrincipalAsync;
            });

        services.AddAuthorization(options =>
            options.AddPolicy(
                AuthPolicies.Productos,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(AuthClaimTypes.Modulo, ModuloAcceso.Productos)));
        services.AddScoped<ApiSessionService>();

        return services;
    }

    private static async Task ValidatePrincipalAsync(
        CookieValidatePrincipalContext context)
    {
        var idClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(idClaim, out var usuarioId) || usuarioId <= 0)
        {
            await RejectAsync(context);
            return;
        }

        var repository = context.HttpContext.RequestServices
            .GetRequiredService<IUsuarioRepository>();
        var tieneAcceso = await repository.TieneAccesoModuloAsync(
            usuarioId,
            ModuloAcceso.Productos,
            context.HttpContext.RequestAborted);

        if (!tieneAcceso)
        {
            await RejectAsync(context);
        }
    }

    private static async Task RejectAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
