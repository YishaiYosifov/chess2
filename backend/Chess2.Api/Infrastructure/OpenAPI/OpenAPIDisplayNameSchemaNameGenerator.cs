using System.ComponentModel;
using System.Reflection;
using NJsonSchema.Generation;

namespace Chess2.Api.Infrastructure.OpenAPI;

public class OpenAPIDisplayNameSchemaNameGenerator : ISchemaNameGenerator
{
    public string Generate(Type type)
    {
        var displayNameAttr = type.GetCustomAttribute<DisplayNameAttribute>();
        if (displayNameAttr != null)
        {
            return displayNameAttr.DisplayName;
        }

        // Default logic for generic types (like PagedResult<User>)
        if (type.IsGenericType)
        {
            var baseName = type.Name[..type.Name.IndexOf('`')];
            var genericArgs = string.Join("And", type.GetGenericArguments().Select(t => t.Name));
            return $"{baseName}Of{genericArgs}";
        }

        return type.Name;
    }
}
