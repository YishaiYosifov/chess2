using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AnarchyChess.Api.Infrastructure;

public class StructStringValueConverter<T> : ValueConverter<T, string>
    where T : struct
{
    private static readonly ConstructorInfo Ctor =
        typeof(T).GetConstructor([typeof(string)])
        ?? throw new InvalidOperationException($"{typeof(T)} must have a string constructor");

    private static readonly Func<string, T> _factory = CreateFactory();

    private static Func<string, T> CreateFactory()
    {
        var param = Expression.Parameter(typeof(string), "value");
        var body = Expression.New(Ctor, param);
        var lambda = Expression.Lambda<Func<string, T>>(body, param);
        return lambda.Compile();
    }

    public StructStringValueConverter()
        : base(x => x.ToString() ?? "", x => _factory(x)) { }
}
