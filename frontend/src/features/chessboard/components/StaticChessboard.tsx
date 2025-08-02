"use client";

import { StoreApi } from "zustand";

import constants from "@/lib/constants";
import { type PieceMap } from "../lib/types";

import { GameColor } from "@/lib/apiClient";
import {
    ChessboardState,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import ChessboardLayout, {
    ChessboardBreakpoint,
    PaddingOffset,
} from "./ChessboardLayout";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { createMoveOptions } from "../lib/moveOptions";
import useConst from "@/hooks/useConst";

export interface ChessboardProps {
    breakpoints?: ChessboardBreakpoint[];
    defaultOffset?: PaddingOffset;

    startingPieces?: PieceMap;
    boardWidth?: number;
    boardHeight?: number;
    viewingFrom?: GameColor;

    className?: string;
}

const StaticChessboard = ({
    breakpoints = [],
    defaultOffset,

    startingPieces = constants.DEFAULT_CHESS_BOARD,
    boardHeight = constants.BOARD_HEIGHT,
    boardWidth = constants.BOARD_WIDTH,

    viewingFrom = GameColor.WHITE,

    className,
}: ChessboardProps) => {
    const chessboardStore = useConst<StoreApi<ChessboardState>>(() =>
        createChessboardStore({
            pieces: startingPieces,
            boardDimensions: { width: boardWidth, height: boardHeight },
            moveOptions: createMoveOptions(),
            viewingFrom,
        }),
    );

    return (
        <ChessboardStoreContext.Provider value={chessboardStore}>
            <ChessboardLayout
                breakpoints={breakpoints}
                defaultOffset={defaultOffset}
                className={className}
            />
        </ChessboardStoreContext.Provider>
    );
};

export default StaticChessboard;
