﻿using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class NoCaptureRuleTests : MovementBasedPieceRulesTestBase
{
    [Fact]
    public void Evaluate_returns_only_moves_to_empty_squares()
    {
        ChessBoard board = new();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        board.PlacePiece(Destinations[0], PieceFactory.Black());
        board.PlacePiece(Destinations[1], PieceFactory.White());

        var behaviour = new NoCaptureRule(MovementMocks);
        var result = behaviour.Evaluate(board, Origin, piece).ToList();

        var expected = Destinations[2..].Select(x => new Move(Origin, x, piece));
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Evaluate_returns_empty_if_all_targets_are_occupied()
    {
        ChessBoard board = new();
        var piece = PieceFactory.White();
        board.PlacePiece(Origin, piece);

        for (var i = 0; i < Destinations.Length; i++)
        {
            board.PlacePiece(
                Destinations[i],
                i % 2 == 0 ? PieceFactory.White() : PieceFactory.Black()
            );
        }

        NoCaptureRule behaviour = new(MovementMocks);
        var result = behaviour.Evaluate(board, Origin, piece);

        result.Should().BeEmpty();
    }
}
