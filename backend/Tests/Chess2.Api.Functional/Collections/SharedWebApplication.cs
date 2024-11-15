using Chess2.Api.TestInfrastructure;

namespace Chess2.Api.Functional.Collections;

[CollectionDefinition(nameof(SharedWebApplication))]
public class SharedWebApplication : ICollectionFixture<Chess2WebApplicationFactory>;
