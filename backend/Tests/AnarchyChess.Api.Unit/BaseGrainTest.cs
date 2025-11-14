using Orleans.TestKit;

namespace AnarchyChess.Api.Unit;

public class BaseGrainTest : TestKitBase
{
    protected static CancellationToken CT => TestContext.Current.CancellationToken;
}
