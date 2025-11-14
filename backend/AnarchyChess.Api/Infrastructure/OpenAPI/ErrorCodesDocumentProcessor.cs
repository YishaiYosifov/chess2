using System.Reflection;
using System.Text.Json;
using AnarchyChess.Api.Infrastructure.Errors;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NJsonSchema;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace AnarchyChess.Api.Infrastructure.OpenAPI;

/// <summary>
/// Unfortunately <see cref="Error.Code" /> is a string so openapi doesn't know what it can be.
///
/// This transformer will add all values from <see cref="ErrorCodes" > to the schema.
///
/// If someone doesn't use the error codes from <see cref="ErrorCodes" />
/// it's not the end of the world because they will be reminded once it is used in the front.
/// But please always use <see cref="ErrorCodes" />.
/// </summary>
public class ErrorCodesDocumentProcessor(
    ILogger<ErrorCodesDocumentProcessor> logger,
    IOptions<JsonOptions> jsonOptions
) : IDocumentProcessor
{
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.JsonSerializerOptions;
    private readonly ILogger<ErrorCodesDocumentProcessor> _logger = logger;

    public void Process(DocumentProcessorContext context)
    {
        var schema = context
            .Document.Definitions.FirstOrDefault(kvp =>
                kvp.Value.Type == JsonObjectType.Object && kvp.Key.Contains(nameof(ApiProblemError))
            )
            .Value;

        if (schema == null)
        {
            _logger.LogWarning(
                "Could not find schema for type {ApiProblemError}",
                nameof(ApiProblemError)
            );
            return;
        }

        var codePropertyName = nameof(ApiProblemError.ErrorCode);
        var jsonCodeProperty =
            _jsonOptions.PropertyNamingPolicy?.ConvertName(codePropertyName) ?? codePropertyName;

        if (!schema.Properties.TryGetValue(jsonCodeProperty, out var codeProperty))
        {
            _logger.LogWarning(
                "Could not find {CodeProperty} on ApiProblemError schema",
                codePropertyName
            );
            return;
        }

        var codes = typeof(ErrorCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => f.GetRawConstantValue() as string)
            .Where(s => s is not null)
            .Distinct();

        codeProperty.Enumeration.Clear();
        foreach (var code in codes)
        {
            codeProperty.Enumeration.Add(code);
        }

        codeProperty.Type = JsonObjectType.String;
    }
}
