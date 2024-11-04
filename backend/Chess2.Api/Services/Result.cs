using Chess2.Api.Errors;

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

    public IResult Match(Func<TValue, IResult> onSuccess) =>
        IsSuccess ? onSuccess(Value) : ToProblemDetails();

    public IResult ToProblemDetails()
    {
        if (Error is null)
            throw new InvalidOperationException();


        return Results.Problem(Error);
    }
}
