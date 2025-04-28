using Microsoft.AspNetCore.Mvc;

namespace Chess2.Api.Models;

public class CustomProblemDetails : ProblemDetails
{
    public IEnumerable<ProblemDetailsError> Errors { get; set; } = [];
}

public class ProblemDetailsError
{
    public required string Code { get; set; }
    public required string Detail { get; set; }
    public Dictionary<string, object>? Metadata { get; set; } = [];
}
