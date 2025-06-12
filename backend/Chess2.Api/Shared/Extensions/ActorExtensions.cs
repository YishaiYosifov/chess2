using Akka.Actor;
using ErrorOr;

namespace Chess2.Api.Shared.Extensions;

public static class ActorExtensions
{
    public static void ReplyWithErrorOr<T>(this IActorRef actor, T value) =>
        actor.Tell(value.ToErrorOr());

    public static void ReplyWithErrorOr<T>(this IActorRef actor, Error value) =>
        actor.Tell(value.OfType<T>());

    public static void ReplyWithErrorOr<T>(this IActorRef actor, IEnumerable<Error> value) =>
        actor.Tell(value.OfType<T>());
}
