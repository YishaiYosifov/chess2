import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";

import {
    type Point,
    type PieceMap,
    type LegalMoves,
    type PieceID,
    Color,
    WSEventOut,
} from "@/models";
import { pointToString, stringToPoint } from "@/lib/utils/chessUtils";
import constants from "@/lib/constants";
import { SendEventMessageFunction } from "@/hooks/useEventWS";

export interface ChessStore {
    viewingFrom: Color;
    playingSide: Color;
    playingAs?: Color;

    boardWidth: number;
    boardHeight: number;

    pieces: PieceMap;
    legalMoves: LegalMoves;

    highlighted: Point[];
    highlightedLegalMoves: Point[];

    selectedPiecePosition?: Point;

    movePiece(from: Point, to: Point): void;
    sendMove(wsSendMethod: SendEventMessageFunction, to: Point): void;
    position2Id(position: Point): PieceID | undefined;
    showLegalMoves(position: Point): void;
}

const defaultState = {
    viewingFrom: Color.White,
    playingSide: Color.White,
    boardWidth: constants.BOARD_WIDTH,
    boardHeight: constants.BOARD_HEIGHT,

    pieces: new Map(),
    legalMoves: {},

    highlighted: [],
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
                const { position2Id } = get();

                const pieceId = position2Id(from);
                if (!pieceId) {
                    console.warn(
                        `Could not move piece from ${from} to ${to} ` +
                            `because no piece was found at ${from}`
                    );
                    return;
                }

                const captureId = position2Id(to);
                set((state) => {
                    state.pieces.get(pieceId)!.position = to;
                    if (captureId) state.pieces.delete(captureId);
                });
            },

            sendMove(wsSendMethod: SendEventMessageFunction, to: Point): void {
                const from = get().selectedPiecePosition;
                if (!from) {
                    console.warn(
                        `Could not send piece movement from ${from} to ${to}` +
                            "because no piece was selected"
                    );
                    return;
                }

                const message = { origin: from, destination: to };
                wsSendMethod(WSEventOut.Move, message);
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
             * @param position - the position of the piece to highlight the legal moves of
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
                    state.selectedPiecePosition = position;
                });
            },
        })),
        shallow
    );
}
