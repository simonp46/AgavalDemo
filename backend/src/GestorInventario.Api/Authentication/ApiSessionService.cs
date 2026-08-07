using System.Globalization;
using System.Security.Claims;
using GestorInventario.Application.Dtos;
using GestorInventario.Application.Exceptions;
using GestorInventario.Domain.Usuarios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GestorInventario.Api.Authentication;

public sealed class ApiSessionService
{
    private static readonly TimeSpan SessionDuration = TimeSpan.FromHours(8);

    public Task SignInAsync(
        HttpContext httpContext,
        UsuarioDto usuario,
        CancellationToken cancellationToken)
    {
        var claims = new Claim[]
        {
            new(
                ClaimTypes.NameIdentifier,
                usuario.Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new(ClaimTypes.GivenName, usuario.Nombre),
            new(ClaimTypes.Surname, usuario.Apellidos),
            new(AuthClaimTypes.Area, usuario.Area),
            new(AuthClaimTypes.Modulo, ModuloAcceso.Productos)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties
        {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.Add(SessionDuration),
            IsPersistent = false
        };

        return httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);
    }

    public Task SignOutAsync(HttpContext httpContext)
    {
        return httpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public int GetUsuarioId(ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var id) ||
            id <= 0)
        {
            throw new AuthenticationRequiredException();
        }

        return id;
    }
}
