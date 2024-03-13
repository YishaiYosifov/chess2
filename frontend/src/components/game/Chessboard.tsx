"use client";

import { useEffect, useMemo, useState } from "react";

import { BOARD_HEIGHT, BOARD_WIDTH, defaultChessBoard } from "@/lib/constants";
import { Variant, Color, ChessBoard, Piece } from "./chess.types";
import styles from "./Chessboard.module.scss";
import ChessPiece from "./ChessPiece";
import { ChessProvider } from "@/contexts/chessStoreContext";

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
 * @param offsetBreakpoints - the offset for each dimention of the screen.
 *  for example, if the screen is 1920x1080 and the current breakpoint width offset is 500,
 *  it will parse the width as 1420 before choosing the board size.
 *  The largest width breakpoint will be used for any screen size larger than it.
 * @returns
 */
const Chessboard = ({
    offsetBreakpoints = [],
    startingPieces = defaultChessBoard,
    boardHeight = BOARD_HEIGHT,
    boardWidth = BOARD_WIDTH,
    viewingFrom = Color.White,
    fixed = false,
}: {
    offsetBreakpoints?: Breakpoint[];
    startingPieces?: ChessBoard;
    boardWidth?: number;
    boardHeight?: number;
    fixed?: boolean;
    viewingFrom?: Color;
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

    useEffect(() => {
        /**
         * Calculate the width and height offset based on the offsetBreakpoints param and window width
         */
        function calculateOffset(): { width: number; height: number } {
            const width = window.innerWidth;
            for (const { widthBreakpoint, offset } of sortedBreakpoints) {
                if (widthBreakpoint > width) return offset;
            }

            return sortedBreakpoints.at(-1)?.offset || { width: 0, height: 0 };
        }

        /**
         * Set the board size based on the viewport size and the offset
         */
        function resizeBoard(): void {
            const { width: offsetWidth, height: offsetHeight } =
                calculateOffset();
            const width = window.innerWidth - offsetWidth;
            const height = window.innerHeight - offsetHeight;

            const minSize = Math.min(width, height);
            setBoardSize(minSize);
        }

        window.addEventListener("resize", resizeBoard);
        resizeBoard();

        return () => window.removeEventListener("resize", resizeBoard);
    }, [sortedBreakpoints]);

    const idPieces = useMemo(() => {
        const idBoard = new Map<string, Piece>();
        startingPieces.forEach((piece, i) => idBoard.set(i.toString(), piece));

        return idBoard;
    }, [startingPieces]);

    return (
        <div
            data-testid="chessboard"
            className={styles.chessboard}
            style={{
                width: `${boardSize}px`,
                height: `${boardSize}px`,
            }}
        >
            <ChessProvider
                pieces={idPieces}
                viewingFrom={viewingFrom}
                fixed={fixed}
                boardWidth={boardWidth}
                boardHeight={boardHeight}
            >
                {[...idPieces].map(([id]) => (
                    <ChessPiece id={id} key={id} />
                ))}
            </ChessProvider>
        </div>
    );
};

export default Chessboard;
