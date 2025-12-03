using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.QuestLogic.MoveConditions;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.QuestLogicTests.MoveConditionTests;

public class IsMoveOfTypeTests
{
    [Fact]
    public void Evaluate_returns_true_for_matching_move_type()
    {
        var move = new MoveFaker().RuleFor(x => x.SpecialMoveType, SpecialMoveType.KingsideCastle);
        new IsMoveOfType(SpecialMoveType.KingsideCastle, SpecialMoveType.QueensideCastle)
            .Evaluate(move)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void Evaluate_returns_false_for_non_matching_move_type()
    {
        var move = new MoveFaker().RuleFor(x => x.SpecialMoveType, SpecialMoveType.IlVaticano);
        new IsMoveOfType(SpecialMoveType.KingsideCastle, SpecialMoveType.QueensideCastle)
            .Evaluate(move)
            .Should()
            .BeFalse();
    }
}
