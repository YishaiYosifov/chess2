using ErrorOr;

namespace Chess2.Api.Models.DTOs;

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
