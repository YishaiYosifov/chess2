import { createWithEqualityFn } from "zustand/traditional";
import { devtools } from "zustand/middleware";
import { shallow } from "zustand/shallow";

import { Point, type Piece } from "@/components/game/chess.types";

export interface ChessStore {
    pieces: Map<string, Piece>;
    highlighted: Point[];

    legalMoves: Point[];
    highlightedLegalMoves: Point[];
}

const defaultState: ChessStore = {
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
