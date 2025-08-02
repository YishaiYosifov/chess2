import { createFakePiece } from "@/lib/testUtils/fakers/chessboardFakers";
import { pointToPiece } from "../simulateMove";
import { PieceMap } from "../types";
import { logicalPoint } from "@/lib/utils/pointUtils";

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
