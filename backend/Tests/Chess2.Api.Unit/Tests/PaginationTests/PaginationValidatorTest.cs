using Chess2.Api.Pagination.Models;
using Chess2.Api.Pagination.Validators;
using FluentValidation.TestHelper;

namespace Chess2.Api.Unit.Tests.PaginationTests;

public class PaginationValidatorTests
{
    private readonly PaginationValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    [InlineData(100)]
    public void PaginationValidator_accepts_page_over_one(int page)
    {
        PaginationQuery model = new(page, 10);
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void PaginationValidator_rejects_pages_under_one(int page)
    {
        PaginationQuery model = new(page, 10);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(25)]
    [InlineData(50)]
    public void PaginationValidator_accepts_page_size_within_range(int pageSize)
    {
        PaginationQuery model = new(0, pageSize);
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(51)]
    [InlineData(100)]
    public void PaginationValidator_rejects_page_size_outside_range(int pageSize)
    {
        PaginationQuery model = new(0, pageSize);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}
