"use client";

import { useEffect, useMemo, useState } from "react";

import { BOARD_HEIGHT, BOARD_WIDTH, defaultChessBoard } from "@/lib/constants";
import {
    type Player,
    type PieceMap,
    type LegalMoves,
    Color,
} from "@/lib/models";
import styles from "./Chessboard.module.scss";

import { ChessProvider } from "@/contexts/chessStoreContext";
import PieceRenderer from "./PieceRenderer";

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
 * @param playingSide - the color of the side whose turn it is to play
 * @param playingAs - the color of the player that is controlling the chessboard.
 *  leave undefined if no player should be controlling this chessboard, thus making it a fixed position
 */
const Chessboard = ({
    offsetBreakpoints = [],
    startingPieces = defaultChessBoard,
    boardHeight = BOARD_HEIGHT,
    boardWidth = BOARD_WIDTH,
    legalMoves = {},

    viewingFrom,
    playingSide,
    playingAs,
}: {
    offsetBreakpoints?: Breakpoint[];
    startingPieces?: PieceMap;
    boardWidth?: number;
    boardHeight?: number;
    legalMoves?: LegalMoves;

    viewingFrom?: Color;
    playingSide?: Color;
    playingAs?: Color;
}) => {
    const [boardSize, setBoardSize] = useState<number>(0);

    // Sort the offset breakpoints in ascending order
    const sortedBreakpoints = useMemo(
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

    viewingFrom ??= playingAs;
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
                pieces={startingPieces}
                legalMoves={legalMoves}
                viewingFrom={viewingFrom}
                playingSide={playingSide}
                playingAs={playingAs}
                boardWidth={boardWidth}
                boardHeight={boardHeight}
            >
                <PieceRenderer />
            </ChessProvider>
        </div>
    );
};

export default Chessboard;
