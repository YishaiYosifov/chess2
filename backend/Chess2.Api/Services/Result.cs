using Chess2.Api.Errors;

namespace Chess2.Api.Services;

public class Result<TValue>
{
    private Result(TValue value)
    {
        Value = value;
    }

    private Result(Error error)
    {
        Error = error;
    }

    public TValue? Value { get; }
    public Error? Error { get; }
    public bool IsSuccess => Error == null;

    public static Result<TValue> Success(TValue value) => new(value);
    public static Result<TValue> Failure(Error error) => new(error);

    public IResult ToResult()
    {
        if (Error is null) return CreateSuccessResult();
        else
        {
            return Results.Problem(
                statusCode: Error.StatusCode,
                title: Error.Title,
                type: Error.Type,
                extensions: new Dictionary<string, object?>
                {
                    { "code", Error.Code },
                    { "description", Error.Description }
                }
            );
        }
    }

    private IResult CreateSuccessResult()
    {
        if (Value is null) return Results.NoContent();
        else return Results.Ok(Value);
    }
}
