import { render, screen } from "@testing-library/react";

import { Color, PieceMap, PieceType, Piece, Point } from "../chess.types";
import ChessPiece from "../ChessPiece";
import { ChessProvider } from "@/contexts/chessStoreContext";

describe("ChessPiece", () => {
    it.each([
        [
            [0, 0],
            [900, 900],
        ],
        [
            [1, 1],
            [800, 800],
        ],
        [
            [0, 5],
            [900, 400],
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
