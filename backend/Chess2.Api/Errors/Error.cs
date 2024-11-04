using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Errors;

public class Error : ProblemDetails
{
    public string Code { get; set; }
    public string Description { get; set; }

    public Error(string code, string description, int statusCode, string title, string type)
    {
        Code = code;
        Description = description;
        Title = title;
        Type = type;
        Status = statusCode;
    }
}

public class BadRequestError(string Code, string Description) : Error(
        Code,
        Description,
        StatusCodes.Status400BadRequest,
        "Bad Request",
        "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1");

public class NotFoundError(string Code, string Description) : Error(
    Code,
    Description,
    StatusCodes.Status404NotFound,
    "Not Found",
    "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5");

public class ConflictError(string Code, string Description) : Error(
    Code,
    Description,
    StatusCodes.Status409Conflict,
    "Conflict", 
    "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.10");
