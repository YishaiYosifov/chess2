"use client";

import { useEffect, useMemo, useState } from "react";

import {
    BOARD_HEIGHT,
    BOARD_WIDTH,
    Color,
    PieceData,
    Variant,
    defaultChessboard,
} from "@/lib/constants";
import styles from "./Chessboard.module.scss";

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
    startingBoard = defaultChessboard,
    boardHeight = BOARD_HEIGHT,
    boardWidth = BOARD_WIDTH,
    side = Color.White,
    fixed = false,
}: {
    variant?: Variant;
    offsetBreakpoints?: Breakpoint[];
    startingBoard?: Record<number, PieceData>;
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
            {Object.entries(startingBoard).map(([i, pieceData]) => {
                // Flip the board depending on which side you are viewing from
                let numIndex = parseInt(i);
                if (side == Color.White)
                    numIndex = boardWidth * boardHeight - 1 - numIndex;

                return (
                    <ChessPiece
                        index={numIndex}
                        pieceData={pieceData}
                        boardWidth={boardWidth}
                        boardHeight={boardHeight}
                        key={i}
                    />
                );
            })}
        </div>
    );
};

export default Chessboard;

export const ChessPiece = ({
    index,
    pieceData,
    boardWidth,
    boardHeight,
}: {
    index: number;
    pieceData: PieceData;
    boardWidth: number;
    boardHeight: number;
}) => {
    const row = Math.floor(index / boardWidth);
    const column = index % boardWidth;
    const boardSize = boardWidth * boardHeight;
    return (
        <div
            data-testid="piece"
            className={styles.piece}
            style={{
                backgroundImage: `url("/assets/pieces/${pieceData.piece}-${pieceData.color}.png")`,
                transform: `translate(${column * boardWidth * boardHeight}%, ${
                    row * boardSize
                }%)`,
            }}
        />
    );
};
