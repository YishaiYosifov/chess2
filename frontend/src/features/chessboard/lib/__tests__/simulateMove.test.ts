import {
    createFakeMove,
    createFakePiece,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { simulateMove, simulateMoveWithIntermediates } from "../simulateMove";
import { logicalPoint } from "@/features/point/pointUtils";
import BoardPieces from "../boardPieces";
import { AnimationStep, IntermediateSquare } from "../types";
import { PieceType, SpecialMoveType } from "@/lib/apiClient";

describe("simulateMove", () => {
    it("should return a new BoardPieces instance that reflects the result of playMove", () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const pieces = BoardPieces.fromPieces(piece);

        const move = createFakeMove({
            from: piece.position,
            to: logicalPoint({ x: 2, y: 2 }),
        });

        const result = simulateMove(pieces, move);

        expect(result.newPieces).not.toBe(pieces);
        expect(result.newPieces.getByPosition(move.to)?.id).toEqual(piece.id);

        // base is unchanged
        expect(pieces.getByPosition(piece.position)?.id).toEqual(piece.id);
        expect(pieces.getByPosition(move.to)).toBeUndefined();

        const expectedPieces = new BoardPieces(pieces);
        expectedPieces.playMove(move);
        const expectedResult: AnimationStep = {
            newPieces: expectedPieces,
            movedPieceIds: [piece.id],
            isCapture: false,
            isPromotion: false,
            specialMoveType: null,
        };
        expect(result).toEqual(expectedResult);
    });

    it("should set initialSpawnPositions only when spawning pieces", () => {
        const newPiece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });

        const piece = createFakePiece({
            position: logicalPoint({ x: 4, y: 4 }),
        });
        const pieces = BoardPieces.fromPieces(piece);

        const move = createFakeMove({
            from: piece.position,
            to: logicalPoint({ x: 5, y: 5 }),
            pieceSpawns: [newPiece],
        });

        const result = simulateMove(pieces, move);

        const expectedInitialSpawnPositions = new BoardPieces(pieces);
        expectedInitialSpawnPositions.addAt(newPiece, move.from);
        expect(result.initialSpawnPositions).toEqual(
            expectedInitialSpawnPositions,
        );

        expect(result.movedPieceIds).toEqual([piece.id, newPiece.id]);
    });

    it("should correctly set isPromotion", () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 5, y: 5 }),
        });
        const pieces = BoardPieces.fromPieces(piece);

        const move = createFakeMove({
            from: piece.position,
            to: logicalPoint({ x: 5, y: 6 }),
            promotesTo: PieceType.QUEEN,
        });

        const result = simulateMove(pieces, move);
        expect(result.isPromotion).toEqual(true);
    });

    it("should set isCapture to true when there are captures", () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const capturedPiece = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const pieces = BoardPieces.fromPieces(piece, capturedPiece);

        const move = createFakeMove({
            from: piece.position,
            to: capturedPiece.position,
            captures: [capturedPiece.position],
        });

        const result = simulateMove(pieces, move);

        expect(result.isCapture).toEqual(true);
    });

    it("should set isCapture to false when captures are already represented in intermediates", () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const capturedPiece = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const pieces = BoardPieces.fromPieces(piece, capturedPiece);

        const move = createFakeMove({
            from: piece.position,
            to: capturedPiece.position,
            captures: [capturedPiece.position],
            intermediates: [
                { position: capturedPiece.position, isCapture: true },
            ],
        });

        const result = simulateMove(pieces, move);

        expect(result.isCapture).toEqual(false);
    });

    it("should forward specialMoveType", () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 7, y: 7 }),
        });
        const pieces = BoardPieces.fromPieces(piece);

        const move = createFakeMove({
            from: piece.position,
            to: logicalPoint({ x: 7, y: 6 }),
            specialMoveType: SpecialMoveType.EN_PASSANT,
        });

        const result = simulateMove(pieces, move);

        expect(result.specialMoveType).toEqual(move.specialMoveType);
    });
});

describe("simulateMoveWithIntermediates", () => {
    it("should return intermediate positions and final move", () => {
        const movingPiece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const pieces = BoardPieces.fromPieces(movingPiece);

        const intermediates: IntermediateSquare[] = [
            { position: logicalPoint({ x: 1, y: 1 }), isCapture: false },
            { position: logicalPoint({ x: 2, y: 2 }), isCapture: true },
        ];
        const move = createFakeMove({
            from: movingPiece.position,
            to: logicalPoint({ x: 3, y: 3 }),
            intermediates,
            captures: [],
        });

        const results = simulateMoveWithIntermediates(pieces, move);

        const expected1 = new BoardPieces(pieces);
        expected1.movePiece(movingPiece.id, intermediates[0].position);
        expect(results.steps[0].newPieces).toEqual(expected1);
        expect(results.steps[0].isCapture).toBe(intermediates[0].isCapture);

        const expected2 = new BoardPieces(pieces);
        expected2.movePiece(movingPiece.id, intermediates[1].position);
        expect(results.steps[1].newPieces).toEqual(expected2);
        expect(results.steps[1].isCapture).toBe(intermediates[1].isCapture);

        const expectedFinal = new BoardPieces(pieces);
        expectedFinal.movePiece(movingPiece.id, move.to);
        expect(results.steps[2].newPieces).toEqual(expectedFinal);

        results.steps.forEach((r) => {
            expect(r.movedPieceIds).toEqual([movingPiece.id]);
        });
    });

    it("should include removedPieceIds from the final move", () => {
        const movingPiece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const capturePiece = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const pieces = BoardPieces.fromPieces(movingPiece, capturePiece);

        const intermediates: IntermediateSquare[] = [
            { position: logicalPoint({ x: 0, y: 1 }), isCapture: false },
            { position: logicalPoint({ x: 1, y: 0 }), isCapture: false },
        ];
        const move = createFakeMove({
            from: movingPiece.position,
            to: capturePiece.position,
            intermediates,
            captures: [capturePiece.position],
        });

        const result = simulateMoveWithIntermediates(pieces, move);

        expect(new Set(...result.removedPieceIds)).toEqual(
            new Set(capturePiece.id),
        );

        expect(result.steps[0].newPieces.getById(capturePiece.id)).toEqual(
            capturePiece,
        );
        expect(result.steps[1].newPieces.getById(capturePiece.id)).toEqual(
            capturePiece,
        );

        expect(result.steps[2].newPieces.getById(capturePiece.id)).toEqual(
            undefined,
        );
    });
});
