using Chess2.Api.TestInfrastructure;

namespace Chess2.Api.Integration;

[Collection(nameof(SharedIntegrationContext))]
public class BaseIntegrationTest(Chess2WebApplicationFactory factory) : ApiTestBase(factory);
