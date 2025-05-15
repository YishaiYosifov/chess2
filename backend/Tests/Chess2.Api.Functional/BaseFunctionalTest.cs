using Chess2.Api.TestInfrastructure;

namespace Chess2.Api.Functional;

[Collection(nameof(SharedFunctionalContext))]
public class BaseFunctionalTest(Chess2WebApplicationFactory factory) : ApiTestBase(factory);
