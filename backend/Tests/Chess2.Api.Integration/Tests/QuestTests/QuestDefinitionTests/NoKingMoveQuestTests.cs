using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestDefinitions;

namespace Chess2.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

public class NoKingMoveQuestTests()
    : NoPieceMoveQuestTestBase<NoKingMoveQuest>(
        forbiddenPiece: PieceType.King,
        allowedPiece: PieceType.Horsey,
        minMoves: 30 * 2,
        expectedDifficulty: QuestDifficulty.Medium,
        target: 2
    );
