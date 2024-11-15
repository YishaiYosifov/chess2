using Chess2.Api.Integration.Collections;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fixtures;

namespace Chess2.Api.Integration.Tests;

[Collection(nameof(SharedIntegrationContext))]
public class BaseIntegrationTest(Chess2WebApplicationFactory factory) : ApiTestFixture(factory);
