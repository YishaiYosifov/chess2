using ErrorOr;

namespace Chess2.Api.Shared.Extensions;

public static class ErrorOrExtensions
{
    public static ErrorOr<T> OfType<T>(this Error error) => error;

    public static ErrorOr<T> OfType<T>(this IEnumerable<Error> error) => error.ToList();
}
