using Chess2.Api.TestInfrastructure;

namespace Chess2.Api.Functional.Collections;

[CollectionDefinition(nameof(SharedFunctionalContext))]
public class SharedFunctionalContext : ICollectionFixture<Chess2WebApplicationFactory>;
