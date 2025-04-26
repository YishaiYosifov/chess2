using Chess2.Api.TestInfrastructure;

namespace Chess2.Api.Integration;

[CollectionDefinition(nameof(SharedIntegrationContext))]
public class SharedIntegrationContext : ICollectionFixture<Chess2WebApplicationFactory>;
