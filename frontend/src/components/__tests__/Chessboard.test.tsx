import { render, screen, fireEvent, waitFor } from "@testing-library/react";

import { Color, Piece, PieceData } from "@/lib/constants";
import Chessboard, { ChessPiece } from "../Chessboard";

jest.mock("@/lib/constants", () => ({
    ...jest.requireActual("@/lib/constants"),
    BOARD_HEIGHT: 10,
    BOARD_WIDTH: 10,
    BOARD_SIZE: 100,
}));

const mockBoard: Record<number, PieceData> = {
    0: { piece: Piece.Rook, color: Color.White },
    1: { piece: Piece.Horse, color: Color.White },
    5: { piece: Piece.Rook, color: Color.Black },
};

describe("Chessboard", () => {
    it.each([
        [Color.White, 900, 900],
        [Color.Black, 0, 0],
    ])(
        "should render pieces in the correct order depending on the viewing side",
        (side, firstRow, firstColumn) => {
            render(
                <Chessboard
                    startingBoard={mockBoard}
                    side={side}
                    boardWidth={10}
                    boardHeight={10}
                />
            );

            const piece = screen.getAllByTestId("piece")[0];
            expect(piece).toHaveStyle(
                `transform: translate(${firstRow}%, ${firstColumn}%)`
            );
        }
    );

    it.each([
        // Resize with one breakpoint
        [
            800,
            [{ widthBreakpoint: 768, offset: { width: 20, height: 20 } }],
            780,
        ],

        // Resize with multiple breakpoints
        [
            1200,
            [
                { widthBreakpoint: 768, offset: { width: 20, height: 20 } },
                { widthBreakpoint: 1200, offset: { width: 40, height: 40 } },
                { widthBreakpoint: 992, offset: { width: 30, height: 30 } },
            ],
            1160,
        ],

        // Resize with no breakpoints
        [1500, [], 1500],

        // Resize with larger screen size than any breakpoint
        [
            2000,
            [
                { widthBreakpoint: 768, offset: { width: 20, height: 20 } },
                { widthBreakpoint: 1200, offset: { width: 40, height: 40 } },
            ],
            1960,
        ],
    ])(
        "should resize board on window resize with different breakpoints",
        async (width, breakpoints, expectedSize) => {
            render(
                <Chessboard
                    boardWidth={10}
                    boardHeight={10}
                    offsetBreakpoints={breakpoints}
                />
            );
            const chessboard = screen.getByTestId("chessboard");

            window.innerWidth = width;
            fireEvent(window, new Event("resize"));
            waitFor(() =>
                expect(chessboard.style.width).toBe(`${expectedSize}px`)
            );
        }
    );
});

describe("ChessPiece", () => {
    it.each([
        [0, { x: 0, y: 0 }],
        [11, { x: 100, y: 100 }],
        [50, { x: 0, y: 500 }],
    ])("should be in the correct position", (index, position) => {
        const pieceData = { piece: Piece.Pawn, color: Color.White };
        render(
            <ChessPiece
                index={index}
                pieceData={pieceData}
                boardHeight={10}
                boardWidth={10}
            />
        );

        const piece = screen.getByTestId("piece");
        expect(piece).toHaveStyle(
            `background-image: url("/assets/pieces/${pieceData.piece}-${pieceData.color}.png");
            transform: translate(${position.x}%, ${position.y}%);`
        );
    });
});
