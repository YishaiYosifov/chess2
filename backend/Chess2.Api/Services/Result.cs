using System.Diagnostics;

namespace Chess2.Api.Services;

public class Result<TValue>
{
    private readonly TValue? _value;
    public Error? Error { get; }

    public bool IsSuccess => Error == null;
    public TValue Value
    {
        get
        {
            return _value
                ?? throw new ArgumentNullException("value");
        }
    }

    protected Result(TValue value)
    {
        _value = value;
    }

    protected Result(Error error)
    {
        Error = error;
    }

    public static Result<TValue> Success(TValue value) => new(value);
    public static Result<TValue> Failure(Error error) => new(error);

    public IResult ToResult()
    {
        if (Error is null)
            return Results.Ok(Value);


        return Results.Problem(
            statusCode: Error.StatusCode,
            title: Error.Title,
            type: Error.Type,
            extensions: new Dictionary<string, object?>
            {
                { "code", Error.Code },
                { "description", Error.Description },
                { "traceId", Activity.Current?.Id }
            }
        );
    }
}
