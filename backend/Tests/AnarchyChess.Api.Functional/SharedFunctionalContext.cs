using AnarchyChess.Api.TestInfrastructure;

namespace AnarchyChess.Api.Functional;

[CollectionDefinition(nameof(SharedFunctionalContext))]
public class SharedFunctionalContext : ICollectionFixture<AnarchyChessWebApplicationFactory>;
