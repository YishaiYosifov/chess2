import {
    createFakeMoveFromPieces,
    createFakePiece,
    createFakePieceMapFromPieces,
} from "@/lib/testUtils/fakers/chessboardFakers";
import {
    pointToPiece,
    simulateMove,
    simulateMoveWithIntermediates,
} from "../simulateMove";
import { PieceMap } from "../types";
import { logicalPoint } from "@/lib/utils/pointUtils";

describe("simulateMove", () => {
    it("should move a piece without captures", () => {
        const piece1 = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const piece2 = createFakePiece({
            position: logicalPoint({ x: 5, y: 5 }),
        });
        const pieces = createFakePieceMapFromPieces(piece1, piece2);

        const move = createFakeMoveFromPieces(pieces, {
            from: piece1.position,
            to: logicalPoint({ x: 1, y: 1 }),
            captures: [],
        });

        const expected = new Map(pieces);
        expected.set("0", { ...piece1, position: move.to });

        const result = simulateMove(pieces, move);
        expect(result.newPieces).toEqual(expected);
        expect(result.movedPieceIds).toEqual(new Set(["0"]));
    });

    it("should remove captured pieces", () => {
        const piece1 = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const piece2 = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const piece3 = createFakePiece({
            position: logicalPoint({ x: 2, y: 2 }),
        });
        const pieces = createFakePieceMapFromPieces(piece1, piece2, piece3);

        const move = createFakeMoveFromPieces(pieces, {
            from: piece1.position,
            to: piece2.position,
            captures: [],
        });

        const expected = new Map(pieces);
        expected.set("0", { ...piece1, position: move.to });
        expected.delete("1");

        const result = simulateMove(pieces, move);
        expect(result.newPieces).toEqual(expected);
        expect(result.movedPieceIds).toEqual(new Set(["0"]));
    });

    it("should apply side effects", () => {
        const piece1 = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const piece2 = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const piece3 = createFakePiece({
            position: logicalPoint({ x: 2, y: 2 }),
        });
        const pieces = createFakePieceMapFromPieces(piece1, piece2, piece3);

        const move = createFakeMoveFromPieces(pieces, {
            from: piece1.position,
            to: logicalPoint({ x: 3, y: 3 }),
            sideEffects: [
                { from: piece2.position, to: logicalPoint({ x: 4, y: 4 }) },
            ],
            captures: [],
        });

        const expected = new Map(pieces);
        expected.set("0", { ...piece1, position: move.to });
        expected.set("1", {
            ...piece2,
            position: logicalPoint({ x: 4, y: 4 }),
        });

        const result = simulateMove(pieces, move);
        expect(result.newPieces).toEqual(expected);
        expect(result.movedPieceIds).toEqual(new Set(["0", "1"]));
    });
});

describe("simulateMoveWithIntermediates", () => {
    it("should return intermediate positions and final move", () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const pieces = createFakePieceMapFromPieces(piece);

        const intermediates = [
            logicalPoint({ x: 1, y: 1 }),
            logicalPoint({ x: 2, y: 2 }),
        ];
        const move = createFakeMoveFromPieces(pieces, {
            from: piece.position,
            to: logicalPoint({ x: 3, y: 3 }),
            intermediates,
            captures: [],
        });

        const results = simulateMoveWithIntermediates(pieces, move);

        const expected1 = new Map(pieces);
        expected1.set("0", { ...piece, position: intermediates[0] });
        expect(results[0].newPieces).toEqual(expected1);

        const expected2 = new Map(pieces);
        expected2.set("0", { ...piece, position: intermediates[1] });
        expect(results[1].newPieces).toEqual(expected2);

        const expectedFinal = new Map(pieces);
        expectedFinal.set("0", { ...piece, position: move.to });
        expect(results[2].newPieces).toEqual(expectedFinal);

        expect(results.every((r) => r.movedPieceIds.has("0"))).toBe(true);
    });
});

describe("pointToPiece", () => {
    it("should return piece ID if piece found at position", () => {
        const piece = createFakePiece();
        const pieces: PieceMap = new Map([["0", piece]]);

        const id = pointToPiece(pieces, piece.position);

        expect(id).toBe("0");
    });

    it("should return undefined if no piece at position", () => {
        const pieces: PieceMap = new Map([["0", createFakePiece()]]);

        const id = pointToPiece(pieces, logicalPoint({ x: 69, y: 420 }));

        expect(id).toBeUndefined();
    });
});
