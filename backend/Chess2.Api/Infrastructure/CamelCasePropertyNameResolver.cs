using FluentValidation.Internal;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Chess2.Api.Infrastructure;

public class CamelCasePropertyNameResolver
{
    public static string? ResolvePropertyName(Type type, MemberInfo memberInfo, LambdaExpression expression)
    {
        return ToCamelCase(
            DefaultPropertyNameResolver(type, memberInfo, expression));
    }

    private static string? DefaultPropertyNameResolver(Type type, MemberInfo memberInfo, LambdaExpression expression)
    {
        if (expression is not null)
        {
            var chain = PropertyChain.FromExpression(expression);
            if (chain.Count > 0) return chain.ToString();
        }

        if (memberInfo is not null)
            return memberInfo.Name;

        return null;
    }

    private static string? ToCamelCase(string? property)
    {
        if (string.IsNullOrEmpty(property) || !char.IsUpper(property[0]))
            return property;

        var chars = property.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (!char.IsUpper(chars[i]))
                break;

            chars[i] = char.ToLower(chars[i], CultureInfo.InvariantCulture);
        }

        return new string(chars);
    }
}