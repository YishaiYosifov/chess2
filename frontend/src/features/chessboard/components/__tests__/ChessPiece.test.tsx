import { render, screen } from "@testing-library/react";

import { PieceMap, PieceType, Piece, Point } from "@/types/tempModels";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import ChessPiece from "../ChessPiece";
import userEvent from "@testing-library/user-event";
import { GameColor } from "@/lib/apiClient";
import { StoreApi } from "zustand";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

describe("ChessPiece", () => {
    const normalize = (str: string) => str.replace(/\s+/g, "");
    function getExpectedTransform(
        physicalPosition: Point,
        draggingOffset: Point,
    ) {
        const expected = `translate(
                clamp(0%, calc(${physicalPosition.x}% + ${draggingOffset.x}px), 900%),
                clamp(0%, calc(${physicalPosition.y}% + ${draggingOffset.y}px), 900%))`;
        return normalize(expected);
    }

    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
        vi.useFakeTimers({ toFake: ["requestAnimationFrame"] });
    });

    function renderPiece(position: Point = { x: 0, y: 0 }) {
        const pieceInfo: Piece = {
            position: position,
            type: PieceType.PAWN,
            color: GameColor.WHITE,
        };
        const pieces: PieceMap = new Map([["0", pieceInfo]]);
        store.setState({ pieces });

        const renderResults = render(
            <ChessboardStoreContext.Provider value={store}>
                <ChessPiece id="0" />
            </ChessboardStoreContext.Provider>,
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
            { x: 0, y: 900 },
        ],
        [
            { x: 1, y: 1 },
            { x: 100, y: 800 },
        ],
        [
            { x: 0, y: 5 },
            { x: 0, y: 400 },
        ],
    ])("should be in the correct position", (position, physicalPosition) => {
        const { pieceInfo, piece } = renderPiece(position);

        const expectedTransform = getExpectedTransform(physicalPosition, {
            x: 0,
            y: 0,
        });
        expect(piece).toHaveStyle(`
            background-image: url("/assets/pieces/${pieceInfo.type}${pieceInfo.color}.png");
        `);
        expect(normalize(piece.style.transform)).toBe(expectedTransform);
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
        vi.advanceTimersToNextFrame();

        const expectedTransform = getExpectedTransform(
            { x: 0, y: 900 },
            mouseCoords,
        );
        expect(normalize(piece.style.transform)).toBe(expectedTransform);
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
        vi.advanceTimersToNextFrame();

        const expectedTransform = getExpectedTransform(
            { x: 0, y: 900 },
            mouseCoords,
        );
        expect(normalize(piece.style.transform)).toBe(expectedTransform);
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

        const expectedTransform = getExpectedTransform(
            { x: 0, y: 900 },
            { x: 0, y: 0 },
        );
        expect(normalize(piece.style.transform)).toBe(expectedTransform);
    });
});
