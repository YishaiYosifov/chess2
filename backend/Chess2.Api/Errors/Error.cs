namespace Chess2.Api;

public record Error(string Code, string Description, int StatusCode, string Title, string Type);

public record BadRequestError(string Code, string Description) : Error(
        Code,
        Description,
        StatusCodes.Status400BadRequest,
        "Bad Request",
        "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1");

public record NotFoundError(string Code, string Description) : Error(
    Code,
    Description,
    StatusCodes.Status404NotFound,
    "Not Found",
    "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5");

public record ConflictError(string Code, string Description) : Error(
    Code,
    Description,
    StatusCodes.Status409Conflict,
    "Conflict", 
    "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.10");
