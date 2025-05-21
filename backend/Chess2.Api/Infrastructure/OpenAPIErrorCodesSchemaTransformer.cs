using System.Reflection;
using System.Text.Json;
using Chess2.Api.Infrastructure.Errors;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Chess2.Api.Infrastructure;

/// <summary>
/// Unfortunately <see cref="Error.Code" /> is a string so openapi doesn't know what it can be.
///
/// This transformer will add all values from <see cref="ErrorCodes" > to the schema.
///
/// If someone doesn't use the error codes from <see cref="ErrorCodes" />
/// it's not the end of the world because they will be reminded once it is used in the front.
/// But please always use <see cref="ErrorCodes" />.
/// </summary>
public class OpenAPIErrorCodesSchemaTransformer(
    ILogger<OpenAPIErrorCodesSchemaTransformer> logger,
    IOptions<JsonOptions> jsonOptions
) : IOpenApiSchemaTransformer
{
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.JsonSerializerOptions;
    private readonly ILogger<OpenAPIErrorCodesSchemaTransformer> _logger = logger;

    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        if (context.JsonTypeInfo.Type != typeof(ApiProblemError))
            return Task.CompletedTask;

        var codePropertyName = nameof(ApiProblemError.ErrorCode);
        var jsonCodeProperty =
            _jsonOptions.PropertyNamingPolicy?.ConvertName(codePropertyName) ?? codePropertyName;
        if (!schema.Properties.TryGetValue(jsonCodeProperty, out var codeProp))
        {
            _logger.LogWarning(
                "Could not find {CodeProperty} on schema properties of type {ApiProblemError}",
                codePropertyName,
                typeof(ApiProblemError).Name
            );
            return Task.CompletedTask;
        }

        var codes = typeof(ErrorCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => f.GetRawConstantValue() as string)
            .Where(s => s is not null)
            .Distinct();
        codeProp.Enum = [.. codes.Select(c => new OpenApiString(c) as IOpenApiAny)];
        codeProp.Type = "enum";
        return Task.CompletedTask;
    }
}
