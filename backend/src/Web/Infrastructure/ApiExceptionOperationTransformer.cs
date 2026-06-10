using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PromptifyWebApi.Web.Infrastructure;

/// <summary>
/// Adds standard error responses to every OpenAPI operation. A 400 Bad Request is added to all
/// operations because every request passes through <c>ValidationBehaviour</c> in the MediatR
/// pipeline. 401 Unauthorized and 403 Forbidden are added only to operations that carry
/// <see cref="IAuthorizeData"/> metadata. Session-scoped routes also document 404 Not Found;
/// <c>POST .../prompts</c> documents 409 Conflict for the session idle gate.
/// </summary>
internal sealed class ApiExceptionOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        operation.Responses ??= [];
        operation.Responses.TryAdd("400", new OpenApiResponse { Description = "Bad Request" });

        var requiresAuth = context.Description.ActionDescriptor.EndpointMetadata
            .Any(m => m is IAuthorizeData);

        if (requiresAuth)
        {
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
        }

        var relativePath = context.Description.RelativePath ?? string.Empty;

        if (relativePath.Contains("{sessionId}", StringComparison.OrdinalIgnoreCase))
        {
            operation.Responses.TryAdd("404", new OpenApiResponse { Description = "Not Found" });
        }

        if (relativePath.Contains("{sessionId}/prompts", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(context.Description.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            operation.Responses.TryAdd("409", new OpenApiResponse { Description = "Conflict" });
        }

        return Task.CompletedTask;
    }
}
