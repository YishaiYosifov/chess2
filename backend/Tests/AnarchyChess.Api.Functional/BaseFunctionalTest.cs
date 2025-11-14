using AnarchyChess.Api.TestInfrastructure;

namespace AnarchyChess.Api.Functional;

[Collection(nameof(SharedFunctionalContext))]
public class BaseFunctionalTest(AnarchyChessWebApplicationFactory factory) : ApiTestBase(factory);
