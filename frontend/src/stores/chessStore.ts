import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";

import { type Point, type Piece, Color } from "@/components/game/chess.types";
import constants from "@/lib/constants";
import { enableMapSet } from "immer";

export interface ChessStore {
    viewingFrom: Color;
    boardWidth: number;
    boardHeight: number;
    fixed: boolean;

    pieces: Map<string, Piece>;
    highlighted: Point[];

    legalMoves: Point[];
    highlightedLegalMoves: Point[];

    movePiece(from: Point, to: Point): void;
    position2Id(position: Point): string | undefined;
}

const defaultState = {
    viewingFrom: Color.White,
    boardWidth: constants.BOARD_WIDTH,
    boardHeight: constants.BOARD_HEIGHT,
    fixed: false,

    pieces: new Map(),
    highlighted: [],

    legalMoves: [],
    highlightedLegalMoves: [],
};

enableMapSet();
export function createChessStore(initState: Partial<ChessStore> = {}) {
    return createWithEqualityFn<ChessStore>()(
        immer((set, get) => ({
            ...defaultState,
            ...initState,

            movePiece(from, to) {
                const pieceId = get().position2Id(from);
                if (!pieceId) return;

                set((state) => {
                    state.pieces.get(pieceId)!.position = to;
                });
            },

            position2Id(position: Point): string | undefined {
                const pieces = get().pieces;
                for (const [id, piece] of pieces) {
                    if (
                        piece.position[0] == position[0] &&
                        piece.position[1] == position[1]
                    )
                        return id;
                }
            },
        })),
        shallow
    );
}
