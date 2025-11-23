import {
    createFakeMove,
    createFakePiece,
} from "@/lib/testUtils/fakers/chessboardFakers";
import BoardPieces from "../boardPieces";
import { logicalPoint } from "@/features/point/pointUtils";
import { PieceType } from "@/lib/apiClient";

describe("BoardPieces", () => {
    describe("playMove", () => {
        it("should move a piece to a new position without captures", () => {
            const movingPiece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const unrelatedPiece = createFakePiece({
                position: logicalPoint({ x: 5, y: 5 }),
            });
            const pieces = BoardPieces.fromPieces(movingPiece, unrelatedPiece);

            const move = createFakeMove({
                from: movingPiece.position,
                to: logicalPoint({ x: 1, y: 1 }),
            });

            const result = pieces.playMove(move);

            expect(pieces.getById(movingPiece.id)?.position).toEqual(
                logicalPoint({ x: 1, y: 1 }),
            );
            expect(result.movedPieceIds).toEqual([movingPiece.id]);
            expect(result.removedPieceIds).toHaveLength(0);
            expect(pieces.getById(unrelatedPiece.id)).toEqual(unrelatedPiece);
        });

        it("should remove captured pieces", () => {
            const movingPiece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const capturedPiece = createFakePiece({
                position: logicalPoint({ x: 1, y: 1 }),
            });
            const pieces = BoardPieces.fromPieces(movingPiece, capturedPiece);

            const move = createFakeMove({
                from: movingPiece.position,
                to: capturedPiece.position,
                captures: [capturedPiece.position],
            });

            const result = pieces.playMove(move);

            expect(pieces.getById(movingPiece.id)?.position).toEqual(
                logicalPoint({ x: 1, y: 1 }),
            );
            expect(pieces.getById(capturedPiece.id)).toBeUndefined();
            expect(result.movedPieceIds).toEqual([movingPiece.id]);
            expect(result.removedPieceIds).toEqual([capturedPiece.id]);
        });

        it("should apply side effects correctly", () => {
            const movingPiece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const sideEffectPiece = createFakePiece({
                position: logicalPoint({ x: 1, y: 1 }),
            });
            const pieces = BoardPieces.fromPieces(movingPiece, sideEffectPiece);

            const move = createFakeMove({
                from: movingPiece.position,
                to: logicalPoint({ x: 2, y: 2 }),
                sideEffects: [
                    {
                        from: sideEffectPiece.position,
                        to: logicalPoint({ x: 3, y: 3 }),
                    },
                ],
            });

            const result = pieces.playMove(move);

            expect(pieces.getById(movingPiece.id)?.position).toEqual(
                logicalPoint({ x: 2, y: 2 }),
            );
            expect(pieces.getById(sideEffectPiece.id)?.position).toEqual(
                logicalPoint({ x: 3, y: 3 }),
            );
            expect(result.movedPieceIds).toEqual([
                movingPiece.id,
                sideEffectPiece.id,
            ]);
            expect(result.removedPieceIds).toHaveLength(0);
        });

        it("should promote a piece if promotesTo is set", () => {
            const movingPiece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
                type: PieceType.PAWN,
            });
            const pieces = BoardPieces.fromPieces(movingPiece);

            const move = createFakeMove({
                from: movingPiece.position,
                to: logicalPoint({ x: 0, y: 7 }),
                promotesTo: PieceType.QUEEN,
            });

            const result = pieces.playMove(move);

            expect(pieces.getById(movingPiece.id)?.type).toEqual(
                PieceType.QUEEN,
            );
            expect(result.movedPieceIds).toEqual([movingPiece.id]);
            expect(result.removedPieceIds).toHaveLength(0);
        });

        it("should spawn new pieces correctly", () => {
            const movingPiece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const spawn1 = createFakePiece({
                position: logicalPoint({ x: 2, y: 2 }),
            });
            const spawn2 = createFakePiece({
                position: logicalPoint({ x: 3, y: 3 }),
            });
            const pieces = BoardPieces.fromPieces(movingPiece);

            const move = createFakeMove({
                from: movingPiece.position,
                to: logicalPoint({ x: 1, y: 1 }),
                pieceSpawns: [spawn1, spawn2],
            });

            const result = pieces.playMove(move);

            expect(pieces.getById(spawn1.id)).toEqual(spawn1);
            expect(pieces.getById(spawn2.id)).toEqual(spawn2);
            expect(result.movedPieceIds).toEqual([
                movingPiece.id,
                spawn1.id,
                spawn2.id,
            ]);
        });

        it("should handle main piece swapping position with side effect piece", () => {
            const mainPiece = createFakePiece({
                position: logicalPoint({ x: 0, y: 0 }),
            });
            const sideEffectPiece = createFakePiece({
                position: logicalPoint({ x: 3, y: 3 }),
            });
            const pieces = BoardPieces.fromPieces(mainPiece, sideEffectPiece);

            const move = createFakeMove({
                from: mainPiece.position,
                to: sideEffectPiece.position,
                sideEffects: [
                    {
                        from: sideEffectPiece.position,
                        to: mainPiece.position,
                    },
                ],
            });

            const result = pieces.playMove(move);

            expect(pieces.getById(mainPiece.id)?.position).toEqual(
                sideEffectPiece.position,
            );
            expect(pieces.getById(sideEffectPiece.id)?.position).toEqual(
                mainPiece.position,
            );
            expect(pieces.getByPosition(mainPiece.position)?.id).toEqual(
                sideEffectPiece.id,
            );
            expect(pieces.getByPosition(sideEffectPiece.position)?.id).toEqual(
                mainPiece.id,
            );

            expect(result.removedPieceIds).toHaveLength(0);

            expect(result.movedPieceIds).toEqual([
                mainPiece.id,
                sideEffectPiece.id,
            ]);
        });

        it("should handle self captures", () => {
            const piece = createFakePiece();
            const pieces = BoardPieces.fromPieces(piece);

            const move = createFakeMove({
                from: piece.position,
                to: piece.position,
                captures: [piece.position],
            });

            const result = pieces.playMove(move);

            expect(result.removedPieceIds).toEqual([piece.id]);

            expect(pieces.getById(piece.id)).toBeUndefined();
            expect(pieces.getByPosition(move.to)).toBeUndefined();
        });
    });

    describe("movePiece", () => {
        it("should move a piece to an empty square", () => {
            const from = logicalPoint({ x: 1, y: 1 });
            const to = logicalPoint({ x: 4, y: 4 });

            const piece = createFakePiece({ position: from });
            const pieces = BoardPieces.fromPieces(piece);

            pieces.movePiece(piece.id, to);

            expect(pieces.getById(piece.id)?.position).toEqual(to);
            expect(pieces.getByPosition(to)?.id).toEqual(piece.id);
            expect(pieces.getByPosition(from)).toBeUndefined();
        });

        it("should delete a piece occupying the destination square", () => {
            const from = logicalPoint({ x: 0, y: 0 });
            const to = logicalPoint({ x: 2, y: 2 });

            const movingPiece = createFakePiece({ position: from });
            const capturedPiece = createFakePiece({ position: to });

            const pieces = BoardPieces.fromPieces(movingPiece, capturedPiece);

            pieces.movePiece(movingPiece.id, to);

            expect(pieces.getById(movingPiece.id)?.position).toEqual(to);
            expect(pieces.getById(capturedPiece.id)).toBeUndefined();
            expect(pieces.getByPosition(to)?.id).toEqual(movingPiece.id);
            expect(pieces.getByPosition(from)).toBeUndefined();
        });
    });

    it("should add a piece and be retrievable by id and position", () => {
        const position = logicalPoint({ x: 1, y: 2 });
        const piece = createFakePiece({ position });
        const board = new BoardPieces();

        board.add(piece);

        const byId = board.getById(piece.id);
        const byPos = board.getByPosition(position);

        expect(byId).toEqual(piece);
        expect(byPos).toEqual(piece);

        // copy
        expect(byId).not.toBe(piece);
        expect(byPos).not.toBe(piece);
    });

    it("should add a piece at a specific position and copy it", () => {
        const originalPos = logicalPoint({ x: 0, y: 0 });
        const piece = createFakePiece({ position: originalPos });
        const board = new BoardPieces();
        const customPos = logicalPoint({ x: 5, y: 5 });

        board.addAt(piece, customPos);

        const addedPiece = board.getById(piece.id);
        expect(addedPiece?.position).toEqual(customPos);
        expect(board.getByPosition(customPos)).toEqual(addedPiece);

        expect(piece.position).toEqual(originalPos);
        expect(addedPiece).not.toBe(piece); // copy
    });

    it("should delete a piece by id", () => {
        const piece = createFakePiece({
            position: logicalPoint({ x: 3, y: 3 }),
        });
        const board = new BoardPieces();
        board.add(piece);

        board.delete(piece.id);

        expect(board.getById(piece.id)).toBeUndefined();
        expect(board.getByPosition(piece.position)).toBeUndefined();
    });

    it("should clone itself correctly when constructed from another BoardPieces", () => {
        const piece1 = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const piece2 = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        const board = BoardPieces.fromPieces(piece1, piece2);

        const clone = new BoardPieces(board);

        // original pieces remain accessible
        expect(clone.getById(piece1.id)).toEqual(piece1);
        expect(clone.getById(piece2.id)).toEqual(piece2);

        // modifying clone does not affect original
        const newPos = logicalPoint({ x: 2, y: 2 });
        clone.movePiece(piece1.id, newPos);
        expect(board.getById(piece1.id)?.position).toEqual(piece1.position);
        expect(clone.getById(piece1.id)?.position).toEqual(newPos);
    });

    it("should iterate over all pieces using values()", () => {
        const pieces = [
            createFakePiece({ position: logicalPoint({ x: 1, y: 1 }) }),
            createFakePiece({ position: logicalPoint({ x: 2, y: 2 }) }),
            createFakePiece({ position: logicalPoint({ x: 3, y: 3 }) }),
        ];
        const board = BoardPieces.fromPieces(...pieces);

        const values = new Set(board.values());
        expect(values).toHaveLength(pieces.length);
        for (const piece of pieces) {
            expect(values).toContainEqual(piece);
        }
    });

    it("should iterate over all keys using keys()", () => {
        const pieces = [
            createFakePiece({ position: logicalPoint({ x: 1, y: 1 }) }),
            createFakePiece({ position: logicalPoint({ x: 2, y: 2 }) }),
            createFakePiece({ position: logicalPoint({ x: 3, y: 3 }) }),
        ];
        const board = BoardPieces.fromPieces(...pieces);

        const values = new Set(board.keys());
        expect(values).toHaveLength(pieces.length);
        for (const piece of pieces) {
            expect(values).toContainEqual(piece.id);
        }
    });

    it("should return undefined for getById or getByPosition if piece does not exist", () => {
        const board = new BoardPieces();
        expect(board.getById("nonexistent")).toBeUndefined();
        expect(
            board.getByPosition(logicalPoint({ x: 0, y: 0 })),
        ).toBeUndefined();
    });
});
