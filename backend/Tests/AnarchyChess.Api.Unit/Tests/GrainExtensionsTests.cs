using AnarchyChess.Api.Infrastructure.Extensions;
using NSubstitute;
using Orleans.Streams;

namespace AnarchyChess.Api.Unit.Tests;

public class GrainExtensionsTests
{
    private readonly Func<int, StreamSequenceToken, Task> _callback = (_, _) => Task.CompletedTask;

    [Fact]
    public async Task SubscribeOrResumeAsync_subscribes_when_there_are_no_existing_handles()
    {
        var streamMock = Substitute.For<IAsyncStream<int>>();
        streamMock.GetAllSubscriptionHandles().Returns([]);

        await streamMock.SubscribeOrResumeAsync(_callback);

        await streamMock.Received(1).SubscribeAsync(Arg.Any<IAsyncObserver<int>>());
    }

    [Fact]
    public async Task SubscribeOrResumeAsync_resumes_existing_handles()
    {
        var handleMock1 = Substitute.For<StreamSubscriptionHandle<int>>();
        var handleMock2 = Substitute.For<StreamSubscriptionHandle<int>>();

        var streamMock = Substitute.For<IAsyncStream<int>>();
        streamMock.GetAllSubscriptionHandles().Returns([handleMock1, handleMock2]);

        await streamMock.SubscribeOrResumeAsync(_callback);

        await handleMock1.Received(1).ResumeAsync(Arg.Any<IAsyncObserver<int>>());
        await handleMock2.Received(1).ResumeAsync(Arg.Any<IAsyncObserver<int>>());
        await streamMock.DidNotReceiveWithAnyArgs().SubscribeAsync(default!, default);
    }
}
