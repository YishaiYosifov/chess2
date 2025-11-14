using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace AnarchyChess.Api.Infrastructure;

public class StructStringTypeConverter<T> : TypeConverter
    where T : struct
{
    private static readonly ConstructorInfo Ctor =
        typeof(T).GetConstructor([typeof(string)])
        ?? throw new InvalidOperationException($"{typeof(T)} must have a string constructor");

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value
    ) => value is string str ? (T)Ctor.Invoke([str]) : base.ConvertFrom(context, culture, value);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    public override object? ConvertTo(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object? value,
        Type destinationType
    ) =>
        value is T && destinationType == typeof(string)
            ? value.ToString()
            : base.ConvertTo(context, culture, value, destinationType);
}
