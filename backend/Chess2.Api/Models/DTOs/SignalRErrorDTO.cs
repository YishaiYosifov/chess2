using ErrorOr;

namespace Chess2.Api.Models.DTOs;

public record SignalRError(
    string Code = "General.Failure",
    string Description = "Internal Server Error"
)
{
    public SignalRError(Error error)
        : this(error.Code, error.Description) { }
}
