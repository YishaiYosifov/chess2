using NJsonSchema;
using NJsonSchema.Generation;

namespace Chess2.Api.Infrastructure.OpenAPI;

public class MarkAsRequiredIfNonNullableSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        foreach (var prop in context.Schema.ActualProperties.Values)
        {
            if (!prop.IsNullable(SchemaType.OpenApi3))
                prop.IsRequired = true;
        }
    }
}
