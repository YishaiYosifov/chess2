import { render, screen, fireEvent, waitFor } from "@testing-library/react";

import { PieceMap, Color, PieceType } from "../chess.types";
import Chessboard from "../Chessboard";

vi.mock("@/lib/constants", async (importOriginal) => ({
    ...(await importOriginal<typeof import("@/lib/constants")>()),
    BOARD_HEIGHT: 10,
    BOARD_WIDTH: 10,
    BOARD_SIZE: 100,
}));

const mockBoard: PieceMap = new Map([
    [
        "1",
        {
            position: [0, 0],
            pieceType: PieceType.Rook,
            color: Color.White,
        },
    ],
    [
        "2",
        {
            position: [1, 0],
            pieceType: PieceType.Horsie,
            color: Color.White,
        },
    ],
    [
        "3",
        {
            position: [5, 0],
            pieceType: PieceType.Rook,
            color: Color.Black,
        },
    ],
]);

describe("Chessboard", () => {
    it.each([
        [Color.White, 0, 0],
        [Color.Black, 900, 900],
    ])(
        "should render pieces in the correct order depending on the viewing side",
        (side, firstRow, firstColumn) => {
            render(
                <Chessboard
                    startingPieces={mockBoard}
                    viewingFrom={side}
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
