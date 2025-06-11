import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";

import {
    type Point,
    type PieceMap,
    type PieceID,
    LegalMoveMap,
} from "@/types/tempModels";
import { pointToString } from "@/lib/utils/pointUtils";
import constants from "@/lib/constants";
import { GameColor } from "@/lib/apiClient";

export interface ChessStore {
    viewingFrom: GameColor;
    sideToMove: GameColor;
    playingAs?: GameColor;

    boardWidth: number;
    boardHeight: number;

    pieces: PieceMap;
    legalMoves: LegalMoveMap;

    highlighted: Point[];
    highlightedLegalMoves: Point[];

    selectedPiecePosition?: Point;

    onPieceMovement?: (from: Point, to: Point) => Promise<void>;

    movePiece(from: Point, to: Point): Promise<void>;
    executePieceMovement(to: Point): Promise<void>;
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
            async movePiece(from: Point, to: Point): Promise<void> {
                const { position2Id, onPieceMovement } = get();

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

                await onPieceMovement?.(from, to);
            },

            /**
             * Process the execution of a move
             *
             * @param to - the new position of the piece
             * @returns where the piece moved
             */
            async executePieceMovement(to: Point): Promise<void> {
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
                await movePiece(from, to);
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
                        piece.position.x == position.x &&
                        piece.position.y == position.y
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
