using ErrorOr;

namespace Chess2.Api.Shared.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Shared.Models.ErrorOrSurrogate`1")]
public struct ErrorOrSurrogate<TValue>
{
    [Id(0)]
    public bool IsError;

    [Id(1)]
    public TValue Value;

    [Id(2)]
    public List<Error> Errors;
}

[GenerateSerializer]
[Alias("Chess2.Api.Shared.Models.ErrorSurrogate")]
public struct ErrorSurrogate
{
    [Id(0)]
    public string Code;

    [Id(1)]
    public string Description;

    [Id(2)]
    public ErrorType Type;

    [Id(3)]
    public Dictionary<string, string>? Metadata;
}

[RegisterConverter]
public sealed class ErrorOrSurrogateConverter<TValue>
    : IConverter<ErrorOr<TValue>, ErrorOrSurrogate<TValue>>
{
    public ErrorOr<TValue> ConvertFromSurrogate(in ErrorOrSurrogate<TValue> surrogate) =>
        surrogate.IsError ? ErrorOr<TValue>.From(surrogate.Errors ?? []) : surrogate.Value;

    public ErrorOrSurrogate<TValue> ConvertToSurrogate(in ErrorOr<TValue> value) =>
        new()
        {
            IsError = value.IsError,
            Value = value.IsError ? default! : value.Value,
            Errors = value.IsError ? value.ErrorsOrEmptyList : [],
        };
}

[RegisterConverter]
public sealed class ErrorConverter : IConverter<Error, ErrorSurrogate>
{
    public Error ConvertFromSurrogate(in ErrorSurrogate surrogate) =>
        Error.Custom(
            (int)surrogate.Type,
            surrogate.Code,
            surrogate.Description,
            surrogate.Metadata?.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value)
        );

    public ErrorSurrogate ConvertToSurrogate(in Error value) =>
        new()
        {
            Code = value.Code,
            Description = value.Description,
            Type = value.Type,
            Metadata = value.Metadata?.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.ToString() ?? ""
            ),
        };
}

[GenerateSerializer]
[Alias("Chess2.Api.Shared.Models.SuccessSurrogate")]
public struct SuccessSurrogate { }

[GenerateSerializer]
[Alias("Chess2.Api.Shared.Models.CreatedSurrogate")]
public struct CreatedSurrogate { }

[GenerateSerializer]
[Alias("Chess2.Api.Shared.Models.DeletedSurrogate")]
public struct DeletedSurrogate { }

[GenerateSerializer]
[Alias("Chess2.Api.Shared.Models.UpdatedSurrogate")]
public struct UpdatedSurrogate { }

[RegisterConverter]
public sealed class SuccessConverter : IConverter<Success, SuccessSurrogate>
{
    public Success ConvertFromSurrogate(in SuccessSurrogate surrogate) => new();

    public SuccessSurrogate ConvertToSurrogate(in Success value) => new();
}

[RegisterConverter]
public sealed class CreatedConverter : IConverter<Created, CreatedSurrogate>
{
    public Created ConvertFromSurrogate(in CreatedSurrogate surrogate) => new();

    public CreatedSurrogate ConvertToSurrogate(in Created value) => new();
}

[RegisterConverter]
public sealed class DeletedConverter : IConverter<Deleted, DeletedSurrogate>
{
    public Deleted ConvertFromSurrogate(in DeletedSurrogate surrogate) => new();

    public DeletedSurrogate ConvertToSurrogate(in Deleted value) => new();
}

[RegisterConverter]
public sealed class UpdatedConverter : IConverter<Updated, UpdatedSurrogate>
{
    public Updated ConvertFromSurrogate(in UpdatedSurrogate surrogate) => new();

    public UpdatedSurrogate ConvertToSurrogate(in Updated value) => new();
}
