using System.Diagnostics;
using GestorInventario.Application.Exceptions;
using GestorInventario.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ApplicationValidationException =
    GestorInventario.Application.Exceptions.ValidationException;

namespace GestorInventario.Api.Errors;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var descriptor = MapException(exception);

        if (descriptor.Status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Error inesperado. TraceId: {TraceId}",
                httpContext.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Solicitud rechazada con codigo {ErrorCode}. TraceId: {TraceId}",
                descriptor.Code,
                httpContext.TraceIdentifier);
        }

        var problemDetails = new ProblemDetails
        {
            Type = descriptor.Type,
            Title = descriptor.Title,
            Status = descriptor.Status,
            Detail = descriptor.Detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["code"] = descriptor.Code;
        problemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? httpContext.TraceIdentifier;

        if (descriptor.Errors is not null)
        {
            problemDetails.Extensions["errors"] = descriptor.Errors;
        }

        httpContext.Response.StatusCode = descriptor.Status;
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken);

        return true;
    }

    private static ErrorDescriptor MapException(Exception exception)
    {
        return exception switch
        {
            ApplicationValidationException validationException => new ErrorDescriptor(
                StatusCodes.Status400BadRequest,
                "urn:gestor-inventario:problem:validation-error",
                "La solicitud contiene datos invalidos.",
                validationException.Message,
                validationException.Code,
                validationException.Errors),
            DomainException domainException => new ErrorDescriptor(
                StatusCodes.Status400BadRequest,
                "urn:gestor-inventario:problem:validation-error",
                "La solicitud contiene datos invalidos.",
                domainException.Message,
                "validation_error",
                new Dictionary<string, string[]>
                {
                    ["$"] = [domainException.Message]
                }),
            ApplicationExceptionBase applicationException =>
                MapApplicationException(applicationException),
            _ => new ErrorDescriptor(
                StatusCodes.Status500InternalServerError,
                "urn:gestor-inventario:problem:unexpected-error",
                "Ocurrio un error inesperado.",
                "No fue posible completar la solicitud.",
                "unexpected_error")
        };
    }

    private static ErrorDescriptor MapApplicationException(
        ApplicationExceptionBase exception)
    {
        return exception.Code switch
        {
            "invalid_credentials" => new ErrorDescriptor(
                StatusCodes.Status401Unauthorized,
                BuildType(exception.Code),
                "No fue posible iniciar sesión.",
                exception.Message,
                exception.Code),
            "authentication_required" => new ErrorDescriptor(
                StatusCodes.Status401Unauthorized,
                BuildType(exception.Code),
                "Autenticación requerida.",
                exception.Message,
                exception.Code),
            "username_already_exists" => new ErrorDescriptor(
                StatusCodes.Status409Conflict,
                BuildType(exception.Code),
                "El usuario indicado no está disponible.",
                exception.Message,
                exception.Code),
            "document_already_exists" => new ErrorDescriptor(
                StatusCodes.Status409Conflict,
                BuildType(exception.Code),
                "El documento ya está registrado.",
                exception.Message,
                exception.Code),
            "category_reference_invalid" => new ErrorDescriptor(
                StatusCodes.Status400BadRequest,
                BuildType(exception.Code),
                "La categoria indicada no es valida.",
                exception.Message,
                exception.Code,
                exception.Errors),
            "product_not_found" => new ErrorDescriptor(
                StatusCodes.Status404NotFound,
                BuildType(exception.Code),
                "Producto no encontrado.",
                exception.Message,
                exception.Code),
            "insufficient_stock" => new ErrorDescriptor(
                StatusCodes.Status409Conflict,
                BuildType(exception.Code),
                "No es posible completar el ajuste de stock.",
                exception.Message,
                exception.Code),
            "delete_conflict" => new ErrorDescriptor(
                StatusCodes.Status409Conflict,
                BuildType(exception.Code),
                "No es posible eliminar el producto.",
                exception.Message,
                exception.Code),
            _ => MapUnknownApplicationException(exception)
        };
    }

    private static ErrorDescriptor MapUnknownApplicationException(
        ApplicationExceptionBase exception)
    {
        var status = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return new ErrorDescriptor(
            status,
            BuildType(exception.Code),
            "No es posible completar la solicitud.",
            exception.Message,
            exception.Code,
            exception.Errors);
    }

    private static string BuildType(string code)
    {
        return $"urn:gestor-inventario:problem:{code.Replace('_', '-')}";
    }

    private sealed record ErrorDescriptor(
        int Status,
        string Type,
        string Title,
        string Detail,
        string Code,
        IReadOnlyDictionary<string, string[]>? Errors = null);
}
