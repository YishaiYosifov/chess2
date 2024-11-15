using Chess2.Api.Integration.Collections;
using Chess2.Api.TestInfrastructure;

namespace Chess2.Api.Integration.Tests;

[Collection(nameof(SharedIntegrationContext))]
public class BaseIntegrationTest(Chess2WebApplicationFactory factory) : ApiTestBase(factory);
