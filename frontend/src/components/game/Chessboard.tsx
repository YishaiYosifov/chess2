"use client";

import { useEffect, useMemo, useState } from "react";

import { BOARD_HEIGHT, BOARD_WIDTH, defaultChessBoard } from "@/lib/constants";
import { PieceInfo, Variant, Color, ChessBoard } from "./chess.types";
import styles from "./Chessboard.module.scss";
import ChessPiece from "./ChessPiece";

interface Breakpoint {
    widthBreakpoint: number;
    offset: {
        width: number;
        height: number;
    };
}

/**
 * Display a chessboard
 *
 * @param variant - the variant of the game
 * @param offsetBreakpoints - the offset for each dimention of the screen.
 *  for example, if the screen is 1920x1080 and the current breakpoint width offset is 500,
 *  it will parse the width as 1420 before choosing the board size
 * The largest width breakpoint will be used for any screen size larger than it.
 * @returns
 */
const Chessboard = ({
    variant = Variant.Anarchy,
    offsetBreakpoints = [],
    startingBoard = defaultChessBoard,
    boardHeight = BOARD_HEIGHT,
    boardWidth = BOARD_WIDTH,
    side = Color.White,
    fixed = false,
}: {
    variant?: Variant;
    offsetBreakpoints?: Breakpoint[];
    startingBoard?: ChessBoard;
    boardWidth?: number;
    boardHeight?: number;
    fixed?: boolean;
    side?: Color;
}) => {
    const [boardSize, setBoardSize] = useState<number>(0);

    // Sort the offset breakpoints in ascending order
    const sortedBreakpoints = useMemo<Breakpoint[]>(
        () =>
            offsetBreakpoints.sort(
                (a, b) => a.widthBreakpoint - b.widthBreakpoint
            ),
        [offsetBreakpoints]
    );

    /**
     * Calculate the width and height offset based on the offsetBreakpoints param and window width
     */
    function calculateOffset(): { width: number; height: number } {
        const width = window.innerWidth;
        for (const { widthBreakpoint, offset } of sortedBreakpoints) {
            if (widthBreakpoint > width) return offset;
        }

        return offsetBreakpoints.at(-1)?.offset || { width: 0, height: 0 };
    }

    /**
     * Set the board size based on the viewport size and the offset
     */
    function resizeBoard(): void {
        const { width: offsetWidth, height: offsetHeight } = calculateOffset();
        const width = window.innerWidth - offsetWidth;
        const height = window.innerHeight - offsetHeight;

        const minSize = Math.min(width, height);
        setBoardSize(minSize);
    }

    useEffect(() => {
        window.addEventListener("resize", resizeBoard);
        resizeBoard();

        return () => window.removeEventListener("resize", resizeBoard);
    }, []);

    return (
        <div
            data-testid="chessboard"
            className={styles.chessboard}
            style={{
                width: `${boardSize}px`,
                height: `${boardSize}px`,
            }}
        >
            {startingBoard.map(([position, pieceInfo], i) => {
                return (
                    <ChessPiece
                        position={position}
                        pieceInfo={pieceInfo}
                        boardWidth={boardWidth}
                        boardHeight={boardHeight}
                        fixed={fixed}
                        side={side}
                        key={i}
                    />
                );
            })}
        </div>
    );
};

export default Chessboard;
