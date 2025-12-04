import { render, screen, fireEvent, waitFor } from "@testing-library/react";

import StaticChessboard from "../StaticChessboard";
import { GameColor, PieceType } from "@/lib/apiClient";
import { logicalPoint } from "@/features/point/pointUtils";
import BoardPieces from "../../lib/boardPieces";

const mockBoard = BoardPieces.fromPieces(
    {
        id: "1",
        position: logicalPoint({ x: 0, y: 0 }),
        type: PieceType.ROOK,
        color: GameColor.WHITE,
    },
    {
        id: "2",
        position: logicalPoint({ x: 1, y: 0 }),
        type: PieceType.HORSEY,
        color: GameColor.WHITE,
    },
    {
        id: "3",
        position: logicalPoint({ x: 5, y: 0 }),
        type: PieceType.ROOK,
        color: GameColor.BLACK,
    },
);

describe("StaticChessboard", () => {
    it.each([
        [GameColor.WHITE, 0, 900],
        [GameColor.BLACK, 900, 0],
    ])(
        "should render pieces in the correct order depending on the viewing side",
        (side, firstRow, firstColumn) => {
            render(
                <StaticChessboard position={mockBoard} viewingFrom={side} />,
            );

            const piece = screen.getAllByTestId("piece")[0];

            const expected = `translate(
                clamp(0%, calc(${firstRow}% + 0px), 900%),
                clamp(0%, calc(${firstColumn}% + 0px), 900%))`;
            expect(piece).toHaveStyle({ transform: expected });
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
                <StaticChessboard
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
