using Akka.Actor;
using ErrorOr;

namespace Chess2.Api.Shared.Extensions;

public static class ActorExtensions
{
    public static void ReplyWithError(this IActorRef actor, Error value) =>
        actor.Tell(value.OfType<object>());

    public static void ReplyWithError(this IActorRef actor, IEnumerable<Error> value) =>
        actor.Tell(value.OfType<object>());

    /// <summary>
    /// Sends a message to the actor and asynchronously waits for a response.
    /// Expects the response to be either:
    /// 1. An ErrorOr containing either errors or a value of type T,
    /// 2. Or a raw value of type T.
    /// </summary>
    /// <typeparam name="T">The expected success value type</typeparam>
    /// <returns>An ErrorOr<T> representing either the errors or the success value.</returns>
    /// <exception cref="ArgumentException">the actor response type is not compatible with T</exception>
    public static async Task<ErrorOr<T>> AskExpecting<T>(
        this IActorRef actor,
        object message,
        CancellationToken token = default
    )
    {
        var response = await actor.Ask<object>(message, token);
        switch (response)
        {
            case ErrorOr<object> errorOr:
                if (errorOr.IsError)
                    return errorOr.Errors;

                if (errorOr.Value is T typedValue)
                    return typedValue;

                throw new ArgumentException(
                    $"Expected response of type {typeof(T)}, but got {errorOr.Value.GetType()}",
                    nameof(message)
                );

            case T directValue:
                return directValue;

            default:
                throw new ArgumentException(
                    $"Expected response of type {typeof(T)}, but got {response.GetType()}",
                    nameof(message)
                );
        }
    }
}
