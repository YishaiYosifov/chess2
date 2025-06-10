"use client";

import { useEffect, useMemo, useState } from "react";

import constants from "@/lib/constants";
import { LegalMoveMap, type PieceMap } from "@/types/tempModels";

import { ChessProvider } from "@/contexts/chessStoreContext";
import PieceRenderer from "./PieceRenderer";
import clsx from "clsx";
import { GameColor } from "@/lib/apiClient";

interface PaddingOffset {
    width: number;
    height: number;
}

interface Breakpoint {
    maxScreenSize: number;
    paddingOffset: PaddingOffset;
}

/**
 * Display a chessboard
 *
 * @param breakpoints - the offset for each dimention of the screen.
 *  for example, if the screen is 1920x1080 and the current breakpoint width offset is 500,
 *  it will parse the width as 1420 before choosing the board size.
 *  The largest width breakpoint will be used for any screen size larger than it.
 * @param sideToMove - the color of the side whose turn it is to play
 * @param playingAs - the color of the player that is controlling the chessboard.
 *  leave undefined if no player should be controlling this chessboard, thus making it a fixed position
 */
const Chessboard = ({
    breakpoints = [],
    defaultOffset,

    startingPieces = constants.DEFAULT_CHESS_BOARD,
    boardHeight = constants.BOARD_HEIGHT,
    boardWidth = constants.BOARD_WIDTH,
    legalMoves,

    viewingFrom,
    sideToMove,
    playingAs,

    className,
}: {
    breakpoints?: Breakpoint[];
    defaultOffset?: PaddingOffset;

    startingPieces?: PieceMap;
    boardWidth?: number;
    boardHeight?: number;
    legalMoves?: LegalMoveMap;

    viewingFrom?: GameColor;
    sideToMove?: GameColor;
    playingAs?: GameColor;

    className?: string;
}) => {
    const [boardSize, setBoardSize] = useState<number>(0);

    // Sort the offset breakpoints in ascending order
    const sortedBreakpoints = useMemo(
        () => breakpoints.sort((a, b) => a.maxScreenSize - b.maxScreenSize),
        [breakpoints],
    );

    useEffect(() => {
        /**
         * Calculate the width and height offset based on the offsetBreakpoints param and window width
         */
        function calculateOffset(): { width: number; height: number } {
            const width = window.innerWidth;
            for (const { maxScreenSize, paddingOffset } of sortedBreakpoints) {
                if (maxScreenSize > width) return paddingOffset;
            }

            return (
                defaultOffset ?? {
                    width: 0,
                    height: 0,
                }
            );
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
    }, [defaultOffset, sortedBreakpoints]);

    viewingFrom ??= playingAs ?? GameColor.WHITE;
    return (
        <div
            data-testid="chessboard"
            className={clsx(
                `grid-template-rows-10 relative grid min-h-[300px] min-w-[300px] cursor-pointer
                grid-cols-10 rounded-md border-2 border-blue-400 bg-[url(/assets/board.svg)]
                bg-[length:100%] bg-no-repeat`,
                className,
            )}
            style={{
                width: `${boardSize}px`,
                height: `${boardSize}px`,
            }}
        >
            <ChessProvider
                pieces={startingPieces}
                legalMoves={legalMoves}
                viewingFrom={viewingFrom}
                sideToMove={sideToMove}
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
