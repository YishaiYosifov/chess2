using NSubstitute.Core.Arguments;

namespace AnarchyChess.Api.TestInfrastructure.NSubtituteExtenstion;

public static class ArgEx
{
    /// <summary>
    /// Verifies all fluent assertions in the provided action succeed for the argument
    /// </summary>
    public static ref T FluentAssert<T>(Action<T?> assertion) =>
        ref ArgumentMatcher.Enqueue(new FluentAssertionArgumentMatcher<T>(assertion))!;
}
