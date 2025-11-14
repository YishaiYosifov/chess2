using AnarchyChess.Api.Pagination.Models;
using FluentValidation;

namespace AnarchyChess.Api.Pagination.Validators;

public class PaginationValidator : AbstractValidator<PaginationQuery>
{
    public PaginationValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
