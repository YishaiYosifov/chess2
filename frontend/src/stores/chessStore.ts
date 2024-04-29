import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";

import {
    type Point,
    type PieceMap,
    type LegalMoves,
    type PieceID,
    type Player,
    Color,
} from "@/models";
import { pointToString, stringToPoint } from "@/lib/utils/chessUtils";
import constants from "@/lib/constants";

export interface ChessStore {
    viewingFrom: Color;
    playingSide: Color;
    boardWidth: number;
    boardHeight: number;
    playingAs?: Player;

    pieces: PieceMap;
    highlighted: Point[];

    legalMoves: LegalMoves;
    highlightedLegalMoves: Point[];

    movePiece(from: Point, to: Point): void;
    position2Id(position: Point): PieceID | undefined;
    showLegalMoves(position: Point): void;
}

const defaultState = {
    viewingFrom: Color.White,
    playingSide: Color.White,
    boardWidth: constants.BOARD_WIDTH,
    boardHeight: constants.BOARD_HEIGHT,

    pieces: new Map(),
    highlighted: [],

    legalMoves: {},
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
            position2Id(position: Point): PieceID | undefined {
                const pieces = get().pieces;
                for (const [id, piece] of pieces) {
                    if (
                        piece.position[0] == position[0] &&
                        piece.position[1] == position[1]
                    )
                        return id;
                }
            },

            /**
             * Highlight the legal moves of a piece
             *
             * @param pieceId - the id of the piece to highlight moves for
             */
            showLegalMoves(position: Point): void {
                const { legalMoves } = get();

                const positionStr = pointToString(position);
                let toHighlight = legalMoves[positionStr];
                toHighlight ??= [];

                const toHighlightPoints = toHighlight.map((x) =>
                    stringToPoint(x)
                );
                set((state) => {
                    state.highlightedLegalMoves = toHighlightPoints;
                });
            },
        })),
        shallow
    );
}
