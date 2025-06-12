using System.Linq.Expressions;
using Akka.Actor;

namespace Chess2.Api.TestInfrastructure.TestActors;

public class ForwardingParentActor<TActor> : ReceiveActor
    where TActor : ActorBase
{
    private readonly IActorRef _child;
    private readonly IActorRef _probe;

    public ForwardingParentActor(Expression<Func<TActor>> childFactory, IActorRef probe)
    {
        _child = Context.ActorOf(Props.Create(childFactory));
        _probe = probe;

        ReceiveAny(msg =>
        {
            if (Context.Sender == _child)
                _probe.Tell(msg);
            else
                _child.Forward(msg);
        });
    }
}
