using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace GestorInventario.Api.Errors;

internal static class ProblemDetailsServiceExtensions
{
    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                var descriptor = ResolveDefaultDescriptor(
                    context.ProblemDetails.Status ??
                    context.HttpContext.Response.StatusCode);

                context.ProblemDetails.Type = descriptor.Type;
                context.ProblemDetails.Title = descriptor.Title;
                context.ProblemDetails.Detail ??= descriptor.Detail;
                context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
                context.ProblemDetails.Extensions.TryAdd(
                    "traceId",
                    Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
                context.ProblemDetails.Extensions.TryAdd(
                    "code",
                    descriptor.Code);
            };
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => JsonNamingPolicy.CamelCase.ConvertName(entry.Key),
                        entry => entry.Value!.Errors
                            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? "El valor proporcionado no es valido."
                                : error.ErrorMessage)
                            .ToArray());

                var problemDetails = new ProblemDetails
                {
                    Type = "urn:gestor-inventario:problem:validation-error",
                    Title = "La solicitud contiene datos invalidos.",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Corrija los campos indicados e intente nuevamente.",
                    Instance = context.HttpContext.Request.Path
                };

                problemDetails.Extensions["code"] = "validation_error";
                problemDetails.Extensions["traceId"] =
                    Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                problemDetails.Extensions["errors"] = errors;

                return new ObjectResult(problemDetails)
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ContentTypes = { "application/problem+json" }
                };
            };
        });

        return services;
    }

    private static DefaultProblemDescriptor ResolveDefaultDescriptor(int status)
    {
        return status switch
        {
            StatusCodes.Status400BadRequest => new DefaultProblemDescriptor(
                "urn:gestor-inventario:problem:validation-error",
                "La solicitud contiene datos invalidos.",
                "Corrija los datos enviados e intente nuevamente.",
                "validation_error"),
            StatusCodes.Status404NotFound => new DefaultProblemDescriptor(
                "urn:gestor-inventario:problem:product-not-found",
                "Producto no encontrado.",
                "No se encontro el recurso solicitado.",
                "product_not_found"),
            StatusCodes.Status401Unauthorized => new DefaultProblemDescriptor(
                "urn:gestor-inventario:problem:authentication-required",
                "Autenticación requerida.",
                "Inicie sesión para acceder al recurso solicitado.",
                "authentication_required"),
            StatusCodes.Status403Forbidden => new DefaultProblemDescriptor(
                "urn:gestor-inventario:problem:module-access-denied",
                "Acceso denegado.",
                "El usuario no tiene permiso para acceder al módulo.",
                "module_access_denied"),
            _ => new DefaultProblemDescriptor(
                "urn:gestor-inventario:problem:unexpected-error",
                "Ocurrio un error inesperado.",
                "No fue posible completar la solicitud.",
                "unexpected_error")
        };
    }

    private sealed record DefaultProblemDescriptor(
        string Type,
        string Title,
        string Detail,
        string Code);
}
