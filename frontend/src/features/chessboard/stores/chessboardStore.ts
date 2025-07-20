import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";

import { BoardSlice, BoardSliceProps, createBoardSlice } from "./boardSlice";
import { createPiecesSlice, PieceSliceProps, PiecesSlice } from "./piecesSlice";
import {
    createLegalMovesSlice,
    LegalMovesSlice,
    LegalMovesSliceProps,
} from "./legalMovesSlice";
import { CoreSlice, createCoreSlice } from "./coreSlice";
import { GameColor } from "@/lib/apiClient";
import constants from "@/lib/constants";

export type ChessboardState = BoardSlice &
    PiecesSlice &
    LegalMovesSlice &
    CoreSlice;
export type ChessboardProps = BoardSliceProps &
    PieceSliceProps &
    LegalMovesSliceProps;

const defaultChessboardState: ChessboardProps = {
    viewingFrom: GameColor.WHITE,
    boardDimensions: {
        width: constants.BOARD_WIDTH,
        height: constants.BOARD_HEIGHT,
    },
    pieces: new Map(),
    legalMoves: new Map(),
    hasForcedMoves: false,
};

enableMapSet();
export function createChessboardStore(
    initState: ChessboardProps = defaultChessboardState,
) {
    return createWithEqualityFn<ChessboardState>()(
        immer((...a) => ({
            ...createBoardSlice(initState)(...a),
            ...createPiecesSlice(initState)(...a),
            ...createLegalMovesSlice(initState)(...a),
            ...createCoreSlice(...a),
        })),
        shallow,
    );
}
