using Chess2.Api.Pagination.Models;
using FluentValidation;

namespace Chess2.Api.Pagination.Validators;

public class PaginationValidator : AbstractValidator<PaginationQuery>
{
    public PaginationValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).InclusiveBetween(0, 50);
    }
}
