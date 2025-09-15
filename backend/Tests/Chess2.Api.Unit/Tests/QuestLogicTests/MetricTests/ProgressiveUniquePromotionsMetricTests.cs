using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.QuestMetrics;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.QuestLogicTests.MetricTests;

public class ProgressiveUniquePromotionsMetricTests
{
    [Fact]
    public void Evaluate_returns_zero_when_no_promotions()
    {
        var snapshot = new GameQuestSnapshotFaker(GameColor.White).Generate();

        ProgressiveUniquePromotionsMetric metric = new();

        int progress = metric.Evaluate(snapshot);

        progress.Should().Be(0);
    }

    [Fact]
    public void Evaluate_counts_multiple_unique_promotions()
    {
        List<Move> moves =
        [
            new MoveFaker(GameColor.White).RuleFor(x => x.PromotesTo, PieceType.Queen).Generate(),
            new MoveFaker(GameColor.White).RuleFor(x => x.PromotesTo, PieceType.Rook).Generate(),
            new MoveFaker(GameColor.White).RuleFor(x => x.PromotesTo, PieceType.Bishop).Generate(),
        ];

        var snapshot = new GameQuestSnapshotFaker(GameColor.White)
            .RuleFor(x => x.MoveHistory, moves)
            .Generate();

        ProgressiveUniquePromotionsMetric metric = new();

        int progress = metric.Evaluate(snapshot);

        progress.Should().Be(3);
    }

    [Fact]
    public void Evaluate_ignores_duplicate_promotions()
    {
        List<Move> moves =
        [
            new MoveFaker(GameColor.White).RuleFor(x => x.PromotesTo, PieceType.Queen).Generate(),
            new MoveFaker(GameColor.White).RuleFor(x => x.PromotesTo, PieceType.Queen).Generate(),
            new MoveFaker(GameColor.White).RuleFor(x => x.PromotesTo, PieceType.Queen).Generate(),
        ];

        var snapshot = new GameQuestSnapshotFaker(GameColor.White)
            .RuleFor(x => x.MoveHistory, moves)
            .Generate();

        ProgressiveUniquePromotionsMetric metric = new();

        int progress = metric.Evaluate(snapshot);

        progress.Should().Be(1);
    }

    [Fact]
    public void Evaluate_persists_state_across_calls()
    {
        var firstSnapshot = new GameQuestSnapshotFaker(GameColor.White)
            .RuleFor(
                x => x.MoveHistory,
                [
                    new MoveFaker(GameColor.White)
                        .RuleFor(x => x.PromotesTo, PieceType.Queen)
                        .Generate(),
                ]
            )
            .Generate();

        var secondSnapshot = new GameQuestSnapshotFaker(GameColor.White)
            .RuleFor(
                x => x.MoveHistory,
                [
                    new MoveFaker(GameColor.White)
                        .RuleFor(x => x.PromotesTo, PieceType.Queen)
                        .Generate(),
                    new MoveFaker(GameColor.White)
                        .RuleFor(x => x.PromotesTo, PieceType.Rook)
                        .Generate(),
                ]
            )
            .Generate();

        ProgressiveUniquePromotionsMetric metric = new();

        int firstProgress = metric.Evaluate(firstSnapshot);
        int secondProgress = metric.Evaluate(secondSnapshot);

        firstProgress.Should().Be(1);
        secondProgress.Should().Be(1); // only rook was new
    }

    [Fact]
    public void Evaluate_ignores_opponent_promotions()
    {
        var snapshot = new GameQuestSnapshotFaker(GameColor.White)
            .RuleFor(
                x => x.MoveHistory,
                [
                    new MoveFaker(GameColor.Black)
                        .RuleFor(x => x.PromotesTo, PieceType.Queen)
                        .Generate(),
                ]
            )
            .Generate();

        ProgressiveUniquePromotionsMetric metric = new();

        int progress = metric.Evaluate(snapshot);

        progress.Should().Be(0);
    }
}
