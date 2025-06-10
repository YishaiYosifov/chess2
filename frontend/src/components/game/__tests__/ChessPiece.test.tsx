import { render, screen } from "@testing-library/react";

import { PieceMap, PieceType, Piece, Point } from "@/types/tempModels";
import { ChessProvider } from "@/contexts/chessStoreContext";
import ChessPiece from "../ChessPiece";
import userEvent from "@testing-library/user-event";
import { GameColor } from "@/lib/apiClient";

describe("ChessPiece", () => {
    function renderPiece(position: Point = { x: 0, y: 0 }) {
        const pieceInfo: Piece = {
            position: position,
            type: PieceType.PAWN,
            color: GameColor.WHITE,
        };
        const pieces: PieceMap = new Map([["0", pieceInfo]]);

        const renderResults = render(
            <ChessProvider pieces={pieces} playingAs={GameColor.WHITE}>
                <ChessPiece id="0" />
            </ChessProvider>,
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
            { x: 0, y: 0 },
            { x: 0, y: 0 },
        ],
        [
            { x: 1, y: 1 },
            { x: 100, y: 100 },
        ],
        [
            { x: 0, y: 5 },
            { x: 0, y: 500 },
        ],
    ])("should be in the correct position", (position, physicalPosition) => {
        const { pieceInfo, piece } = renderPiece(position as Point);

        expect(piece).toHaveStyle(`
            background-image: url("/assets/pieces/${pieceInfo.type}-${pieceInfo.color}.png");
            transform: translate(${physicalPosition.x}%, ${physicalPosition.y}%);
            left: 0px;
            top: 0px
        `);
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
            top: ${mouseCoords.y}px;`,
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
            top: ${mouseCoords.y}px;`,
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
            top: 0px;`,
        );
    });
});
