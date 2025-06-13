import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";

import {
    type Point,
    type PieceMap,
    type PieceID,
    LegalMoveMap,
    Move,
} from "@/types/tempModels";
import { pointToStr } from "@/lib/utils/pointUtils";
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

    playMove(move: Move): void;
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

            playMove(move: Move): void {
                const { position2Id, playMove } = get();

                const pieceId = position2Id(move.from);
                if (!pieceId) {
                    console.warn(
                        `Could not move piece from ${pointToStr(move.from)} to ${pointToStr(move.to)} ` +
                            `because no piece was found at ${pointToStr(move.from)}`,
                    );
                    return;
                }

                const captureIds = [position2Id(move.to)];
                for (const capture of move.captures)
                    captureIds.push(position2Id(capture));

                set((state) => {
                    state.pieces.get(pieceId)!.position = move.to;
                    for (const captureId of captureIds) {
                        if (captureId) state.pieces.delete(captureId);
                    }
                });

                for (const sideEffect of move.sideEffects) playMove(sideEffect);
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
                    onPieceMovement,
                    playMove,
                    legalMoves,
                    clearLegalMoves,
                } = get();
                if (!from) {
                    console.warn(
                        `Could not send piece movement from ${from} to ${to}` +
                            "because no piece was selected",
                    );
                    return;
                }

                const positionStr = pointToStr(from);
                const moves = legalMoves.get(positionStr);
                const move = moves?.find(
                    (m) => m.to.x == to.x && m.to.y == to.y,
                );
                if (!move) {
                    console.warn(
                        `Could not move piece from ${from} to ${to} because no legal move found`,
                    );
                    return;
                }

                playMove(move);
                clearLegalMoves();
                await onPieceMovement?.(from, to);
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

                const positionStr = pointToStr(position);
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
