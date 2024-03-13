import { createWithEqualityFn } from "zustand/traditional";
import { devtools } from "zustand/middleware";
import { shallow } from "zustand/shallow";

import { type Point, type Piece, Color } from "@/components/game/chess.types";
import constants from "@/lib/constants";

export interface ChessStore {
    viewingFrom: Color;
    boardWidth: number;
    boardHeight: number;
    fixed: boolean;

    pieces: Map<string, Piece>;
    highlighted: Point[];

    legalMoves: Point[];
    highlightedLegalMoves: Point[];
}

const defaultState: ChessStore = {
    viewingFrom: Color.White,
    boardWidth: constants.BOARD_WIDTH,
    boardHeight: constants.BOARD_HEIGHT,
    fixed: false,

    pieces: new Map(),
    highlighted: [],

    legalMoves: [],
    highlightedLegalMoves: [],
};

export function createChessStore(initState: Partial<ChessStore> = {}) {
    return createWithEqualityFn<ChessStore>()(
        devtools(() => ({ ...defaultState, ...initState })),
        shallow
    );
}
