import { render, screen } from "@testing-library/react";

import { Color, PieceMap, PieceType, Piece, Point } from "../chess.types";
import { ChessProvider } from "@/contexts/chessStoreContext";
import ChessPiece from "../ChessPiece";

describe("ChessPiece", () => {
    it.each([
        [
            [0, 0],
            [0, 0],
        ],
        [
            [1, 1],
            [100, 100],
        ],
        [
            [0, 5],
            [0, 500],
        ],
    ])("should be in the correct position", (position, physicalPosition) => {
        const pieceInfo: Piece = {
            position: position as Point,
            pieceType: PieceType.Pawn,
            color: Color.White,
        };
        const pieces: PieceMap = new Map([["0", pieceInfo]]);

        render(
            <ChessProvider pieces={pieces}>
                <ChessPiece id="0" />
            </ChessProvider>
        );

        const piece = screen.getByTestId("piece");
        expect(piece).toHaveStyle(
            `background-image: url("/assets/pieces/${pieceInfo.pieceType}-${pieceInfo.color}.png");
            transform: translate(${physicalPosition[0]}%, ${physicalPosition[1]}%);`
        );
    });
});
