import {
    createFakeMoveFromPieces,
    createFakePiece,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { simulateMove, simulateMoveWithIntermediates } from "../simulateMove";
import { logicalPoint } from "@/features/point/pointUtils";
import BoardPieces from "../boardPieces";
import { IntermediateSquare } from "../types";

describe("simulateMove", () => {
    it("should move a piece without captures", () => {
        const movingPiece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const unrelatedPiece = createFakePiece({
            position: logicalPoint({ x: 5, y: 5 }),
        });
        const pieces = BoardPieces.fromPieces(movingPiece, unrelatedPiece);

        const move = createFakeMoveFromPieces(pieces, {
            from: movingPiece.position,
            to: logicalPoint({ x: 1, y: 1 }),
            captures: [],
        });

        const expected = new BoardPieces(pieces);
        expected.move(movingPiece.id, move.to);

        const result = simulateMove(pieces, move);
        expect(result.newPieces).toEqual(expected);
        expect(result.movedPieceIds).toEqual([movingPiece.id]);
    });

    it("should remove captured pieces", () => {
        const movingPiece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const capturePiece = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const unrelatedPiece = createFakePiece({
            position: logicalPoint({ x: 2, y: 2 }),
        });
        const pieces = BoardPieces.fromPieces(
            movingPiece,
            capturePiece,
            unrelatedPiece,
        );

        const move = createFakeMoveFromPieces(pieces, {
            from: movingPiece.position,
            to: capturePiece.position,
            captures: [],
        });

        const expected = new BoardPieces(pieces);
        expected.move(movingPiece.id, move.to);
        expected.delete(capturePiece.id);

        const result = simulateMove(pieces, move);
        expect(result.newPieces).toEqual(expected);
        expect(result.movedPieceIds).toEqual([movingPiece.id]);
    });

    it("should apply side effects", () => {
        const movingPiece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const sideEffectPiece = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const unrelatedPiece = createFakePiece({
            position: logicalPoint({ x: 2, y: 2 }),
        });
        const pieces = BoardPieces.fromPieces(
            movingPiece,
            sideEffectPiece,
            unrelatedPiece,
        );

        const move = createFakeMoveFromPieces(pieces, {
            from: movingPiece.position,
            to: logicalPoint({ x: 3, y: 3 }),
            sideEffects: [
                {
                    from: sideEffectPiece.position,
                    to: logicalPoint({ x: 4, y: 4 }),
                },
            ],
            captures: [],
        });

        const expected = new BoardPieces(pieces);
        expected.move(movingPiece.id, move.to);
        expected.move(sideEffectPiece.id, move.sideEffects[0].to);

        const result = simulateMove(pieces, move);
        expect(result.newPieces).toEqual(expected);
        expect(result.movedPieceIds).toEqual([
            movingPiece.id,
            sideEffectPiece.id,
        ]);
    });

    it("should not delete a side-effected piece that moves away from the destination square", () => {
        const movingPiece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const sideEffectPiece = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const anotherPiece = createFakePiece({
            position: logicalPoint({ x: 2, y: 2 }),
        });
        const pieces = BoardPieces.fromPieces(
            movingPiece,
            sideEffectPiece,
            anotherPiece,
        );

        const move = createFakeMoveFromPieces(pieces, {
            from: movingPiece.position,
            to: sideEffectPiece.position,
            sideEffects: [
                {
                    from: sideEffectPiece.position,
                    to: logicalPoint({ x: 2, y: 2 }),
                },
            ],
            captures: [],
        });

        const expected = new BoardPieces(pieces);
        expected.move(movingPiece.id, move.to);
        expected.move(sideEffectPiece.id, move.sideEffects[0].to);

        const result = simulateMove(pieces, move);
        // make sure it's still the same order
        expect([...result.newPieces]).toEqual([...expected]);
        expect(result.movedPieceIds).toEqual([
            movingPiece.id,
            sideEffectPiece.id,
        ]);
    });

    it("should spawn new pieces", () => {
        const movingPiece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const pieces = BoardPieces.fromPieces(movingPiece);

        const spawn1 = createFakePiece({
            position: logicalPoint({ x: 2, y: 2 }),
        });
        const spawn2 = createFakePiece({
            position: logicalPoint({ x: 3, y: 3 }),
        });

        const move = createFakeMoveFromPieces(pieces, {
            from: movingPiece.position,
            to: logicalPoint({ x: 1, y: 1 }),
            pieceSpawns: [spawn1, spawn2],
        });

        const result = simulateMove(pieces, move);

        const expected = new BoardPieces(pieces);
        expected.move(movingPiece.id, move.to);
        expected.add(spawn1);
        expected.add(spawn2);
        expect(result.newPieces).toEqual(expected);

        const expectedInitialSpawnPositions = new BoardPieces(pieces);
        expectedInitialSpawnPositions.addAt(spawn1, move.from);
        expectedInitialSpawnPositions.addAt(spawn2, move.from);
        expect(result.initialSpawnPositions).toEqual(
            expectedInitialSpawnPositions,
        );
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
        const move = createFakeMoveFromPieces(pieces, {
            from: movingPiece.position,
            to: logicalPoint({ x: 3, y: 3 }),
            intermediates,
            captures: [],
        });

        const results = simulateMoveWithIntermediates(pieces, move);

        const expected1 = new BoardPieces(pieces);
        expected1.move(movingPiece.id, intermediates[0].position);
        expect(results.steps[0].newPieces).toEqual(expected1);
        expect(results.steps[0].isCapture).toBe(intermediates[0].isCapture);

        const expected2 = new BoardPieces(pieces);
        expected2.move(movingPiece.id, intermediates[1].position);
        expect(results.steps[1].newPieces).toEqual(expected2);
        expect(results.steps[1].isCapture).toBe(intermediates[1].isCapture);

        const expectedFinal = new BoardPieces(pieces);
        expectedFinal.move(movingPiece.id, move.to);
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
        const move = createFakeMoveFromPieces(pieces, {
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
