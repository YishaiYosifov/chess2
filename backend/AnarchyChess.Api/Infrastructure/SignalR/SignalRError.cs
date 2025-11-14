using ErrorOr;

namespace AnarchyChess.Api.Infrastructure.SignalR;

public class SignalRError
{
    public string Code { get; set; } = "General.Failure";
    public string Description { get; set; } = "Internal Server Error";

    public SignalRError() { }

    public SignalRError(Error error)
    {
        Code = error.Code;
        Description = error.Description;
    }
}
