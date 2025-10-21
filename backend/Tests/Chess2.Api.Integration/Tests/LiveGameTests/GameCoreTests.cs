using Chess2.Api.GameLogic.Models;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.TestInfrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.LiveGameTests;

public class GameCoreTests : BaseIntegrationTest
{
    private readonly IGameCore _gameCore;
    private readonly GameResultDescriber _resultDescriber = new();
    private readonly GameCoreState _state = new();

    public GameCoreTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _gameCore = Scope.ServiceProvider.GetRequiredService<IGameCore>();
        _gameCore.StartGame(_state);
    }

    [Fact]
    public void MakeMove_moves_the_piece_and_updates_legal_moves()
    {
        var moveKey = new MoveKey(new("e2"), new("e4"));
        var result = _gameCore.MakeMove(moveKey, _state);

        result.IsError.Should().BeFalse();
        _gameCore.SideToMove(_state).Should().Be(GameColor.Black);

        var legalMoves = _gameCore.GetLegalMovesOf(GameColor.Black, _state);
        legalMoves.MoveMap.Should().NotBeEmpty();
    }

    [Fact]
    public void MakeMove_allows_multiple_valid_moves_in_sequence()
    {
        MakeMoves(new MoveKey(new("e2"), new("e4")), new MoveKey(new("e9"), new("e7")));

        _gameCore.SideToMove(_state).Should().Be(GameColor.White);
    }

    [Fact]
    public void MakeMove_detects_draw_if_occurs()
    {
        List<MoveKey> repetitionMoves =
        [
            new MoveKey(new("b1"), new("c3")),
            new MoveKey(new("b10"), new("c8")),
            new MoveKey(new("c3"), new("b1")),
            new MoveKey(new("c8"), new("b10")),
        ];

        for (int i = 0; i < 3; i++)
        {
            MakeMoves(repetitionMoves);
        }

        var result = MakeMoves(repetitionMoves);
        result.EndStatus.Should().Be(_resultDescriber.ThreeFold());
    }

    [Fact]
    public void MakeMove_detects_forced_moves()
    {
        MakeMoves(
            new MoveKey(new("f2"), new("f5")),
            new MoveKey(new("f9"), new("f6")),
            new MoveKey(new("g1"), new("c5")),
            new MoveKey(new("a9"), new("a8"))
        );

        var legalMoves = _gameCore.GetLegalMovesOf(GameColor.White, _state);
        legalMoves.HasForcedMoves.Should().BeTrue();
        legalMoves.MovePaths.Should().ContainSingle();
        legalMoves.MoveMap.Should().ContainSingle();
    }

    [Fact]
    public void MakeMove_detects_king_capture()
    {
        var result = MakeMoves(
            new MoveKey(new("f2"), new("f5")),
            new MoveKey(new("g9"), new("g7")),
            new MoveKey(new("e1"), new("j6")),
            new MoveKey(new("j9"), new("j8")),
            new MoveKey(new("j6"), new("f10"))
        );
        result.EndStatus.Should().Be(_resultDescriber.KingCaptured(GameColor.White));
        result.San.Should().Be("Qxf10#");
    }

    private MoveResult MakeMoves(params IEnumerable<MoveKey> moves)
    {
        MoveResult lastResult = default;
        foreach (var move in moves)
        {
            var result = _gameCore.MakeMove(move, _state);
            result.IsError.Should().BeFalse();
            lastResult = result.Value;
        }

        return lastResult;
    }
}
