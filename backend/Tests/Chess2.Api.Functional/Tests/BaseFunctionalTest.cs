using Chess2.Api.TestInfrastructure;

namespace Chess2.Api.Functional.Tests;

[Collection(nameof(SharedFunctionalContext))]
public class BaseFunctionalTest(Chess2WebApplicationFactory factory) : ApiTestBase(factory);
