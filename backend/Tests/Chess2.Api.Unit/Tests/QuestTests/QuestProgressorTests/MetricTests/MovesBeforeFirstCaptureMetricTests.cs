using Chess2.Api.GameLogic.Models;
using Chess2.Api.Quests.QuestProgressors.Metrics;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.QuestTests.QuestProgressorTests.MetricTests;

public class MovesBeforeFirstCaptureMetricTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void EvaluateProgressMade_with_no_captures_returns_the_total_moves(int moveCount)
    {
        var moves = new MoveSnapshotFaker()
            .RuleFor(m => m.Path, new MovePathFaker().RuleFor(x => x.CapturedIdxs, []))
            .Generate(moveCount);

        var snapshot = new GameStateFaker().RuleFor(x => x.MoveHistory, moves);

        MovesBeforeFirstCaptureMetric metric = new();

        int progressWhite = metric.EvaluateProgressMade(snapshot, GameColor.White);
        int progressBlack = metric.EvaluateProgressMade(snapshot, GameColor.Black);

        progressWhite.Should().Be(moveCount);
        progressBlack.Should().Be(moveCount);
    }

    [Fact]
    public void EvaluateProgressMade_with_the_first_capture_at_first_move_returns_zero()
    {
        var moves = new MoveSnapshotFaker()
            .RuleFor(m => m.Path, new MovePathFaker().RuleFor(x => x.CapturedIdxs, [0]))
            .Generate(1);

        var snapshot = new GameStateFaker().RuleFor(x => x.MoveHistory, moves);

        MovesBeforeFirstCaptureMetric metric = new();

        int progress = metric.EvaluateProgressMade(snapshot, GameColor.White);

        progress.Should().Be(0);
    }

    [Fact]
    public void EvaluateProgressMade_with_the_first_capture_in_middle_returns_the_correct_index()
    {
        var noCaptures = new MoveSnapshotFaker()
            .RuleFor(m => m.Path, new MovePathFaker().RuleFor(x => x.CapturedIdxs, []))
            .Generate(3);

        var captureMove = new MoveSnapshotFaker()
            .RuleFor(m => m.Path, new MovePathFaker().RuleFor(x => x.CapturedIdxs, [5]))
            .Generate();

        var moves = noCaptures.Append(captureMove).ToList();

        var snapshot = new GameStateFaker().RuleFor(x => x.MoveHistory, moves);

        MovesBeforeFirstCaptureMetric metric = new();

        int progress = metric.EvaluateProgressMade(snapshot, GameColor.White);

        progress.Should().Be(3);
    }

    [Fact]
    public void EvaluateProgressMade_with_multiple_captures_returns_the_index_of_first()
    {
        var noCaptures = new MoveSnapshotFaker()
            .RuleFor(m => m.Path, new MovePathFaker().RuleFor(x => x.CapturedIdxs, []))
            .Generate(2);

        var firstCapture = new MoveSnapshotFaker()
            .RuleFor(m => m.Path, new MovePathFaker().RuleFor(x => x.CapturedIdxs, [1]))
            .Generate();

        var secondCapture = new MoveSnapshotFaker()
            .RuleFor(m => m.Path, new MovePathFaker().RuleFor(x => x.CapturedIdxs, [2]))
            .Generate();

        var moves = noCaptures.Concat([firstCapture, secondCapture]).ToList();

        var snapshot = new GameStateFaker().RuleFor(x => x.MoveHistory, moves);

        MovesBeforeFirstCaptureMetric metric = new();

        int progress = metric.EvaluateProgressMade(snapshot, GameColor.Black);

        progress.Should().Be(2);
    }
}
