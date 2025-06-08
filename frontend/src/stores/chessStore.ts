import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";

import {
    type Point,
    type PieceMap,
    type PieceID,
    WSEventOut,
    Move,
    StrPoint,
} from "@/types/tempModels";
import { pointToString, stringToPoint } from "@/lib/utils/chessUtils";
import constants from "@/lib/constants";
import { SendEventMessageFunction } from "@/hooks/useEventWS";
import { GameColor } from "@/lib/apiClient";

export interface ChessStore {
    viewingFrom: GameColor;
    sideToMove: GameColor;
    playingAs?: GameColor;

    boardWidth: number;
    boardHeight: number;

    pieces: PieceMap;
    legalMoves: Map<StrPoint, Move[]>;

    highlighted: Point[];
    highlightedLegalMoves: Point[];

    selectedPiecePosition?: Point;

    movePiece(from: Point, to: Point): void;
    sendMove(wsSendMethod: SendEventMessageFunction, to: Point): void;
    position2Id(position: Point): PieceID | undefined;
    showLegalMoves(position: Point): void;
    clearLegalMoves(): void;
}

const defaultState = {
    viewingFrom: GameColor.WHITE,
    sideToMove: GameColor.WHITE,
    boardWidth: constants.BOARD_WIDTH,
    boardHeight: constants.BOARD_HEIGHT,

    pieces: new Map(),
    legalMoves: new Map(),

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
                            `because no piece was found at ${from}`,
                    );
                    return;
                }

                const captureId = position2Id(to);
                set((state) => {
                    state.pieces.get(pieceId)!.position = to;
                    if (captureId) state.pieces.delete(captureId);
                });
            },

            /**
             * Send a websocket event to move the selected piece to a point
             *
             * @param wsSendMethod - the method to call to send the websocket event
             * @param to - the position to move the piece to
             */
            sendMove(wsSendMethod: SendEventMessageFunction, to: Point): void {
                const {
                    selectedPiecePosition: from,
                    movePiece,
                    clearLegalMoves,
                } = get();
                if (!from) {
                    console.warn(
                        `Could not send piece movement from ${from} to ${to}` +
                            "because no piece was selected",
                    );
                    return;
                }

                clearLegalMoves();
                wsSendMethod(WSEventOut.Move, {
                    origin: from,
                    destination: to,
                });

                movePiece(from, to);
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
                const moves = legalMoves.get(positionStr);

                const toHighlightPoints = moves
                    ? [
                          ...moves.map((m) => m.to),
                          ...moves.map((m) => m.through).flat(),
                      ]
                    : [];

                set((state) => {
                    state.highlightedLegalMoves = toHighlightPoints;
                    state.selectedPiecePosition = position;
                });
            },

            /**
             * Hide all highlighted legal moves
             */
            clearLegalMoves(): void {
                set((state) => {
                    state.highlightedLegalMoves = [];
                    state.selectedPiecePosition = undefined;
                });
            },
        })),
        shallow,
    );
}
