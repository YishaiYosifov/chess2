using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestDefinitions;

namespace AnarchyChess.Api.Integration.Tests.QuestTests.QuestDefinitionTests;

public class NoQueenMoveQuestTests()
    : NoPieceMoveQuestTestBase<NoQueenMoveQuest>(
        forbiddenPiece: PieceType.Queen,
        allowedPiece: PieceType.Horsey,
        minMoves: 30 * 2,
        expectedDifficulty: QuestDifficulty.Medium,
        target: 2
    );
