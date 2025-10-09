using Chess2.Api.Profile.Models;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.ProfileTests;

public class UserIdTests
{
    [Fact]
    public void Guest_creates_an_id_that_starts_with_guest()
    {
        var userId = UserId.Guest();
        userId.Value.Should().StartWith("guest:");
    }

    [Fact]
    public void Guest_returns_unique_id()
    {
        var userId1 = UserId.Guest();
        var userId2 = UserId.Guest();

        userId1.Value.Should().NotBe(userId2.Value);
    }

    [Fact]
    public void IsGuest_returns_true_for_guest()
    {
        var userId = UserId.Guest();
        userId.IsGuest.Should().BeTrue();
    }

    [Fact]
    public void IsGuest_returns_false_for_authed()
    {
        UserId userId = Guid.NewGuid().ToString();
        userId.IsGuest.Should().BeFalse();
    }

    [Fact]
    public void IsAuthed_returns_true_for_authed()
    {
        UserId userId = Guid.NewGuid().ToString();
        userId.IsAuthed.Should().BeTrue();
    }

    [Fact]
    public void IsAuthed_returns_false_for_guest()
    {
        var userId = UserId.Guest();
        userId.IsAuthed.Should().BeFalse();
    }
}
