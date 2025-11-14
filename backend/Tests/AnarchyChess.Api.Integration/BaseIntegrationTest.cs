using AnarchyChess.Api.TestInfrastructure;

namespace AnarchyChess.Api.Integration;

[Collection(nameof(SharedIntegrationContext))]
public class BaseIntegrationTest(AnarchyChessWebApplicationFactory factory) : ApiTestBase(factory);
