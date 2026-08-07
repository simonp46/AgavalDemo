using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GestorInventario.Api.Swagger;

internal sealed class ProblemDetailsContentTypeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses is null)
        {
            return;
        }

        foreach (var responseEntry in operation.Responses)
        {
            if (!int.TryParse(responseEntry.Key, out var statusCode) ||
                statusCode < StatusCodes.Status400BadRequest)
            {
                continue;
            }

            var content = responseEntry.Value?.Content;

            if (content is not null &&
                content.TryGetValue("application/json", out var mediaType))
            {
                content.Remove("application/json");
                content["application/problem+json"] = mediaType;
            }
        }
    }
}
