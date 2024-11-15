using Chess2.Api.Functional.Collections;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fixtures;

namespace Chess2.Api.Functional.Tests;

[Collection(nameof(SharedWebApplication))]
public class BaseFunctionalTest(Chess2WebApplicationFactory factory) : ApiTestFixture(factory);
