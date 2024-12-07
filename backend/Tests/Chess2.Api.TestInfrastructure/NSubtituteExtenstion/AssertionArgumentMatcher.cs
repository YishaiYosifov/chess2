using FluentAssertions.Execution;
using NSubstitute.Core;
using NSubstitute.Core.Arguments;

namespace Chess2.Api.TestInfrastructure.NSubtituteExtenstion;

public class FluentAssertionArgumentMatcher<T>(Action<T> assertion) : IArgumentMatcher<T>, IDescribeNonMatches
{
    private readonly Action<T> _assertion = assertion;
    private string _failedExpectations = string.Empty;

    public string DescribeFor(object? argument) => _failedExpectations;

    /// <summary>
    /// Use fluentassertion to validate the assertion
    /// </summary>
    public bool IsSatisfiedBy(T? value)
    {
        if (value is null)
        {
            _failedExpectations = "Value is null";
            return false;
        }

        using var scope = new AssertionScope();
        _assertion(value);
        _failedExpectations = string.Join(Environment.NewLine, scope.Discard());

        return _failedExpectations.Length == 0;
    }

}