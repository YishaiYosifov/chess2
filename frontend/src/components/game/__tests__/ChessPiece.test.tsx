import { render, screen } from "@testing-library/react";

import { Color, PieceMap, PieceType, Piece, Point } from "../chess.types";
import { ChessProvider } from "@/contexts/chessStoreContext";
import ChessPiece from "../ChessPiece";
import userEvent from "@testing-library/user-event";

describe("ChessPiece", () => {
    function renderPiece(position: Point = [0, 0]) {
        const pieceInfo: Piece = {
            position: position,
            pieceType: PieceType.Pawn,
            color: Color.White,
        };
        const pieces: PieceMap = new Map([["0", pieceInfo]]);

        const renderResults = render(
            <ChessProvider pieces={pieces}>
                <ChessPiece id="0" />
            </ChessProvider>
        );
        const piece = screen.getByTestId("piece");

        return {
            ...renderResults,
            piece,
            pieceInfo,
        };
    }

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
        const { pieceInfo, piece } = renderPiece(position as Point);

        expect(piece).toHaveStyle(
            `background-image: url("/assets/pieces/${pieceInfo.pieceType}-${pieceInfo.color}.png");
            transform: translate(${physicalPosition[0]}%, ${physicalPosition[1]}%);
            left: 0px;
            top: 0px`
        );
    });

    it("should snap to the mouse when clicked", async () => {
        const mouseCoords = { x: 1, y: 2 };

        const user = userEvent.setup();
        const { piece } = renderPiece();

        await user.pointer([
            {
                target: piece,
                coords: mouseCoords,
                keys: "[MouseLeft>]",
            },
        ]);

        expect(piece).toHaveStyle(
            `left: ${mouseCoords.x}px;
            top: ${mouseCoords.y}px;`
        );
    });

    it("should follow the mouse after clicking", async () => {
        const mouseCoords = { x: 69, y: 420 };

        const user = userEvent.setup();
        const { piece } = renderPiece();

        await user.pointer([
            {
                target: piece,
                keys: "[MouseLeft>]",
            },
            { coords: mouseCoords },
        ]);

        expect(piece).toHaveStyle(
            `left: ${mouseCoords.x}px;
            top: ${mouseCoords.y}px;`
        );
    });

    it("should release reset the position of the piece once released", async () => {
        const user = userEvent.setup();
        const { piece } = renderPiece();

        await user.pointer([
            {
                target: piece,
                keys: "[MouseLeft>]",
            },
            { coords: { x: 6, y: 9 } },
            { keys: "[/MouseLeft]" },
        ]);

        expect(piece).toHaveStyle(
            `left: 0px;
            top: 0px;`
        );
    });
});
