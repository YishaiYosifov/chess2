using AnarchyChess.Api.TestInfrastructure;

namespace AnarchyChess.Api.Integration;

[CollectionDefinition(nameof(SharedIntegrationContext))]
public class SharedIntegrationContext : ICollectionFixture<AnarchyChessWebApplicationFactory>;
