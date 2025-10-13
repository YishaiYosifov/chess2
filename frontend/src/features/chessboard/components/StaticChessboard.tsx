"use client";

import { StoreApi } from "zustand";

import constants from "@/lib/constants";
import { type PieceMap } from "../lib/types";

import { GameColor } from "@/lib/apiClient";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import ChessboardLayout, { ChessboardLayoutProps } from "./ChessboardLayout";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import { createMoveOptions } from "../lib/moveOptions";
import useConst from "@/hooks/useConst";

export interface ChessboardProps {
    startingPieces?: PieceMap;
    boardWidth?: number;
    boardHeight?: number;
    viewingFrom?: GameColor;
}

const StaticChessboard = ({
    startingPieces = constants.DEFAULT_CHESS_BOARD,
    boardHeight = constants.BOARD_HEIGHT,
    boardWidth = constants.BOARD_WIDTH,

    viewingFrom = GameColor.WHITE,

    ...props
}: ChessboardProps & ChessboardLayoutProps) => {
    const chessboardStore = useConst<StoreApi<ChessboardStore>>(() =>
        createChessboardStore({
            pieceMap: startingPieces,
            boardDimensions: { width: boardWidth, height: boardHeight },
            moveOptions: createMoveOptions(),
            viewingFrom,
        }),
    );

    return (
        <ChessboardStoreContext.Provider value={chessboardStore}>
            <ChessboardLayout {...props} />
        </ChessboardStoreContext.Provider>
    );
};

export default StaticChessboard;
