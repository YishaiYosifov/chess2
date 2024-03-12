import { render, screen } from "@testing-library/react";

import { Color, PieceInfo, PieceType, Point } from "../chess.types";
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
        const pieceInfo: PieceInfo = {
            pieceType: PieceType.Pawn,
            color: Color.White,
        };
        render(
            <ChessPiece
                position={position as Point}
                pieceInfo={pieceInfo}
                boardHeight={10}
                boardWidth={10}
            />
        );

        const piece = screen.getByTestId("piece");
        expect(piece).toHaveStyle(
            `background-image: url("/assets/pieces/${pieceInfo.pieceType}-${pieceInfo.color}.png");
            transform: translate(${physicalPosition[0]}%, ${physicalPosition[1]}%);`
        );
    });
});
