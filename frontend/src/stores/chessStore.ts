import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";

import {
    type Point,
    type PieceMap,
    Color,
} from "@/components/game/chess.types";
import constants from "@/lib/constants";
import { enableMapSet } from "immer";

export interface ChessStore {
    viewingFrom: Color;
    playingSide: Color;
    boardWidth: number;
    boardHeight: number;
    fixed: boolean;

    pieces: PieceMap;
    highlighted: Point[];

    legalMoves: Point[];
    highlightedLegalMoves: Point[];

    movePiece(from: Point, to: Point): void;
    position2Id(position: Point): string | undefined;
}

const defaultState = {
    viewingFrom: Color.White,
    playingSide: Color.White,
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

            /**
             * Move a piece from one position to another
             *
             * @param from - the current position of the piece
             * @param to - the new position of the piece
             */
            movePiece(from: Point, to: Point): void {
                const pieceId = get().position2Id(from);
                if (!pieceId) return;

                set((state) => {
                    state.pieces.get(pieceId)!.position = to;
                });
            },

            /**
             * Find the id of the piece that is at a certain position
             *
             * @param position - the position to convert to piece id
             * @returns the id of the piece if it was found, undefined otherwise
             */
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
