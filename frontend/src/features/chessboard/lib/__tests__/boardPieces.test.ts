import { createFakePiece } from "@/lib/testUtils/fakers/chessboardFakers";
import BoardPieces from "../boardPieces";
import { logicalPoint } from "@/features/point/pointUtils";
import { PieceType } from "@/lib/apiClient";

describe("BoardPieces", () => {
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

    it("should move a piece to a new position", () => {
        const originalPos = logicalPoint({ x: 2, y: 3 });
        const piece = createFakePiece({ position: originalPos });
        const board = new BoardPieces();
        board.add(piece);

        const newPos = logicalPoint({ x: 4, y: 3 });
        board.move(piece.id, newPos);

        const movedPiece = board.getById(piece.id);
        expect(movedPiece?.position).toEqual(newPos);
        expect(board.getByPosition(newPos)).toEqual(movedPiece);
        expect(board.getByPosition(originalPos)).toBeUndefined();
    });

    it("should move a piece and promote it if specified", () => {
        const originalPos = logicalPoint({ x: 1, y: 1 });
        const piece = createFakePiece({
            position: originalPos,
            type: PieceType.PAWN,
        });
        const board = new BoardPieces();
        board.add(piece);

        const newPos = logicalPoint({ x: 1, y: 2 });
        board.move(piece.id, newPos, PieceType.QUEEN);

        const movedPiece = board.getById(piece.id);
        expect(movedPiece?.position).toEqual(newPos);
        expect(movedPiece?.type).toBe(PieceType.QUEEN);
    });

    it("should remove a piece that is at the new position when another piece moves there", () => {
        const piece1 = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const piece2 = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });

        const board = BoardPieces.fromPieces(piece1, piece2);

        board.move(piece1.id, piece2.position);

        const movedPieceA = board.getById(piece1.id);
        expect(movedPieceA?.position).toEqual(piece2.position);
        expect(board.getByPosition(piece2.position)).toEqual(movedPieceA);

        // piece2 should have been removed
        expect(board.getById(piece2.id)).toBeUndefined();
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
        clone.move(piece1.id, newPos);
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

        const values = Array.from(board.values());
        expect(values).toHaveLength(pieces.length);
        for (const piece of pieces) {
            expect(values).toContainEqual(piece);
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
