import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { devtools } from "zustand/middleware";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";

import { createPiecesSlice, PieceSliceProps, PiecesSlice } from "./piecesSlice";
import { BoardSlice, BoardSliceProps, createBoardSlice } from "./boardSlice";
import {
    createLegalMovesSlice,
    LegalMovesSlice,
    LegalMovesSliceProps,
} from "./legalMovesSlice";
import { OverlaySlice, createOverlaySlice } from "./overlaySlice";
import { CoreSlice, createCoreSlice } from "./coreSlice";
import { GameColor } from "@/lib/apiClient";
import constants from "@/lib/constants";
import { createInteractionSlice, InteractionSlice } from "./interactionSlice";
import { createMoveOptions } from "../lib/moveOptions";
import { createPromotionSlice, PromotionSlice } from "./promotionSlice";
import {
    createIntermediateSlice,
    IntermediateSlice,
} from "./intermediateSlice";
import { AnimationSlice, createAnimationSlice } from "./animationSlice";

export type ChessboardStore = BoardSlice &
    PiecesSlice &
    PromotionSlice &
    LegalMovesSlice &
    OverlaySlice &
    InteractionSlice &
    IntermediateSlice &
    AnimationSlice &
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
    pieceMap: new Map(),
    canDrag: true,
    moveOptions: createMoveOptions(),
};

enableMapSet();
export function createChessboardStore(
    initState: ChessboardProps = defaultChessboardState,
) {
    return createWithEqualityFn<ChessboardStore>()(
        devtools(
            immer((...a) => ({
                ...createBoardSlice(initState)(...a),
                ...createPiecesSlice(initState)(...a),
                ...createPromotionSlice(...a),
                ...createLegalMovesSlice(initState)(...a),
                ...createOverlaySlice(...a),
                ...createInteractionSlice(...a),
                ...createIntermediateSlice(...a),
                ...createAnimationSlice(...a),
                ...createCoreSlice(...a),
            })),
            { name: "chessboardStore" },
        ),
        shallow,
    );
}
