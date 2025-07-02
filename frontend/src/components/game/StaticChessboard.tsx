"use client";

import { useMemo } from "react";

import constants from "@/lib/constants";
import { LegalMoveMap, type PieceMap } from "@/types/tempModels";

import { GameColor } from "@/lib/apiClient";
import { createChessboardStore } from "@/stores/chessboardStore";
import ChessboardLayout, {
    ChessboardBreakpoint,
    PaddingOffset,
} from "./ChessboardLayout";
import { ChessStoreContext } from "@/contexts/chessStoreContext";

export interface ChessboardProps {
    breakpoints?: ChessboardBreakpoint[];
    defaultOffset?: PaddingOffset;

    startingPieces?: PieceMap;
    boardWidth?: number;
    boardHeight?: number;
    legalMoves?: LegalMoveMap;
    viewingFrom?: GameColor;

    className?: string;
}

/**
 * Display a chessboard
 *
 * @param breakpoints - the offset for each dimention of the screen.
 *  for example, if the screen is 1920x1080 and the current breakpoint width offset is 500,
 *  it will parse the width as 1420 before choosing the board size.
 *  The largest width breakpoint will be used for any screen size larger than it.
 */
const StaticChessboard = ({
    breakpoints = [],
    defaultOffset,

    startingPieces = constants.DEFAULT_CHESS_BOARD,
    boardHeight = constants.BOARD_HEIGHT,
    boardWidth = constants.BOARD_WIDTH,
    legalMoves = new Map(),

    viewingFrom = GameColor.WHITE,

    className,
}: ChessboardProps) => {
    const chessStore = useMemo(
        () =>
            createChessboardStore({
                pieces: startingPieces,
                boardDimensions: { width: boardWidth, height: boardHeight },
                legalMoves,
                viewingFrom,
            }),
        [startingPieces, boardWidth, boardHeight, legalMoves, viewingFrom],
    );

    return (
        <ChessStoreContext.Provider value={chessStore}>
            <ChessboardLayout
                breakpoints={breakpoints}
                defaultOffset={defaultOffset}
                className={className}
            />
        </ChessStoreContext.Provider>
    );
};

export default StaticChessboard;
