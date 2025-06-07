using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Chess2.Api.Infrastructure.OpenAPITransformers;

/// <summary>
/// https://github.com/dotnet/aspnetcore/issues/61407#issuecomment-2854726253
/// so stupid...
/// </summary>
public class FixBrokenReferencesInArraysSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(schema);

        if (schema.Properties is null)
        {
            return Task.CompletedTask;
        }

        var propertiesToFix = schema
            .Properties.Select(x => x.Value)
            .Where(x => x.Type == "array")
            .Where(x => x.Items is not null)
            .Where(x => !string.IsNullOrEmpty(x.Items.Reference?.Id))
            .Where(x => x.Items.Annotations is not null);

        foreach (var property in propertiesToFix)
        {
            if (!property.Items.Annotations.TryGetValue("x-schema-id", out var schemaId))
            {
                continue;
            }

            property.Items.Reference = new OpenApiReference
            {
                Type = ReferenceType.Schema,
                Id = schemaId.ToString(),
            };
        }

        return Task.CompletedTask;
    }
}
