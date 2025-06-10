import { render, screen, fireEvent, waitFor } from "@testing-library/react";

import { PieceMap, PieceType } from "@/types/tempModels";
import Chessboard from "../Chessboard";
import { GameColor } from "@/lib/apiClient";

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
            position: { x: 0, y: 0 },
            type: PieceType.ROOK,
            color: GameColor.WHITE,
        },
    ],
    [
        "2",
        {
            position: { x: 1, y: 0 },
            type: PieceType.HORSEY,
            color: GameColor.WHITE,
        },
    ],
    [
        "3",
        {
            position: { x: 5, y: 0 },
            type: PieceType.ROOK,
            color: GameColor.BLACK,
        },
    ],
]);

describe("Chessboard", () => {
    it.each([
        [GameColor.WHITE, 0, 0],
        [GameColor.BLACK, 900, 900],
    ])(
        "should render pieces in the correct order depending on the viewing side",
        (side, firstRow, firstColumn) => {
            render(
                <Chessboard startingPieces={mockBoard} viewingFrom={side} />,
            );

            const piece = screen.getAllByTestId("piece")[0];
            expect(piece).toHaveStyle(
                `transform: translate(${firstRow}%, ${firstColumn}%)`,
            );
        },
    );

    it.each([
        // Resize with one breakpoint
        [800, [], { width: 20, height: 20 }, 780],

        // Resize with multiple breakpoints
        [
            990,
            [
                {
                    maxScreenSize: 768,
                    paddingOffset: { width: 20, height: 20 },
                },
                {
                    maxScreenSize: 992,
                    paddingOffset: { width: 30, height: 30 },
                },
            ],
            { width: 40, height: 40 },
            960,
        ],

        // Resize with no breakpoints
        [1500, [], undefined, 1500],

        // Resize with larger screen size than any breakpoint
        [
            2000,
            [
                {
                    maxScreenSize: 768,
                    paddingOffset: { width: 20, height: 20 },
                },
            ],
            {
                width: 40,
                height: 40,
            },
            1960,
        ],
    ])(
        "should resize board on window resize with different breakpoints",
        async (width, breakpoints, defaultOffset, expectedSize) => {
            render(
                <Chessboard
                    breakpoints={breakpoints}
                    defaultOffset={defaultOffset}
                />,
            );
            const chessboard = screen.getByTestId("chessboard");

            window.innerWidth = width;
            window.innerHeight = width;
            fireEvent(window, new Event("resize"));
            await waitFor(() =>
                expect(chessboard.style.width).toBe(`${expectedSize}px`),
            );
        },
    );
});
