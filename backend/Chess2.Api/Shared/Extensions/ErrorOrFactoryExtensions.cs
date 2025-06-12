using ErrorOr;

namespace Chess2.Api.Shared.Extensions;

public static class ErrorOrFacEx
{
    public static ErrorOr<T> From<T>(Error error) => error;

    public static ErrorOr<T> From<T>(IEnumerable<Error> error) => error.ToList();
}
