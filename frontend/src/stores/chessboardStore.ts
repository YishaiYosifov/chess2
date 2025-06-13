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

export interface BoardDimensions {
    width: number;
    height: number;
}

export interface ChessboardStore {
    viewingFrom: GameColor;
    sideToMove: GameColor;
    playingAs?: GameColor;

    boardDimensions: BoardDimensions;
    boardRect?: DOMRect;

    pieces: PieceMap;
    legalMoves: LegalMoveMap;

    highlighted: Point[];
    highlightedLegalMoves: Point[];
    animatingPieces: Set<PieceID>;

    selectedPieceId?: PieceID;

    onPieceMovement?: (from: Point, to: Point) => Promise<void>;

    playMove(move: Move): void;
    moveSelectedPiece(to: Point): Promise<void>;
    handlePieceDrop(mouseX: number, mouseY: number): Promise<void>;
    position2Id(position: Point): PieceID | undefined;
    showLegalMoves(pieceId: PieceID): void;
    clearLegalMoves(): void;
    addAnimatingPiece(pieceId: PieceID): void;
    setBoardRect(rect: DOMRect): void;
}

const defaultState = {
    viewingFrom: GameColor.WHITE,
    sideToMove: GameColor.WHITE,
    boardDimensions: {
        width: constants.BOARD_WIDTH,
        height: constants.BOARD_HEIGHT,
    },
    physicalBoardDimensions: {
        width: 0,
        height: 0,
    },

    pieces: new Map(),
    legalMoves: new Map(),

    highlighted: [],
    highlightedLegalMoves: [],
    animatingPieces: new Set<PieceID>(),
    boardRef: undefined,
};

enableMapSet();
export function createChessboardStore(
    initState: Partial<ChessboardStore> = {},
) {
    return createWithEqualityFn<ChessboardStore>()(
        immer((set, get) => ({
            ...defaultState,
            ...initState,

            playMove(move: Move): void {
                const { position2Id, addAnimatingPiece, playMove } = get();

                const pieceId = position2Id(move.from);
                if (!pieceId) {
                    console.warn(
                        `Could not move piece from ${pointToStr(move.from)} to ${pointToStr(move.to)} ` +
                            `because no piece was found at ${pointToStr(move.from)}`,
                    );
                    return;
                }
                addAnimatingPiece(pieceId);

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
            async moveSelectedPiece(to: Point): Promise<void> {
                const strTo = pointToStr(to);

                const {
                    selectedPieceId,
                    onPieceMovement,
                    playMove,
                    legalMoves,
                    clearLegalMoves,
                    pieces,
                } = get();
                if (!selectedPieceId) {
                    console.warn(
                        `Could not execute piece movement to ${strTo} ` +
                            "because no piece was selected",
                    );
                    return;
                }

                const selectedPiece = pieces.get(selectedPieceId)!;
                const from = selectedPiece.position;
                const strFrom = pointToStr(from);

                const moves = legalMoves.get(strFrom);
                const move = moves?.find(
                    (m) => m.to.x == to.x && m.to.y == to.y,
                );
                if (!move) {
                    console.warn(
                        `Could not move piece from ${strFrom} to ${strTo} because no legal move found`,
                    );
                    return;
                }

                clearLegalMoves();
                await onPieceMovement?.(from, to);
                playMove(move);
            },

            async handlePieceDrop(
                mouseX: number,
                mouseY: number,
            ): Promise<void> {
                const {
                    boardDimensions,
                    boardRect,
                    viewingFrom,
                    moveSelectedPiece,
                } = get();
                if (!boardRect) {
                    console.warn("Cannot move piece, board rect not set yet");
                    return;
                }

                const relX = Math.max(mouseX - boardRect.left, 0);
                const relY = Math.max(mouseY - boardRect.top, 0);

                let x = Math.floor(
                    (relX / boardRect.width) * boardDimensions.width,
                );
                let y = Math.floor(
                    (relY / boardRect.height) * boardDimensions.height,
                );
                if (viewingFrom == GameColor.WHITE) {
                    y = boardDimensions.height - y - 1;
                } else {
                    x = boardDimensions.width - x - 1;
                }

                await moveSelectedPiece({ x, y });
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
            showLegalMoves(pieceId: PieceID): void {
                const { legalMoves, pieces } = get();
                const piece = pieces.get(pieceId);
                if (!piece) {
                    console.warn(
                        `Cannot show legal moves, no piece was found with id ${pieceId}`,
                    );
                    return;
                }

                const positionStr = pointToStr(piece.position);
                const moves = legalMoves.get(positionStr);

                const toHighlightPoints = moves
                    ? [
                          ...moves.map((m) => m.to),
                          ...moves.map((m) => m.through).flat(),
                      ]
                    : [];

                set((state) => {
                    state.highlightedLegalMoves = toHighlightPoints;
                    state.selectedPieceId = pieceId;
                });
            },

            /**
             * Hide all highlighted legal moves
             */
            clearLegalMoves(): void {
                set((state) => {
                    state.highlightedLegalMoves = [];
                    state.selectedPieceId = undefined;
                });
            },

            addAnimatingPiece(pieceId: PieceID): void {
                set((state) => {
                    if (!state.animatingPieces.has(pieceId))
                        state.animatingPieces.add(pieceId);
                });

                setTimeout(
                    () =>
                        set((state) => {
                            state.animatingPieces.delete(pieceId);
                        }),
                    100,
                );
            },

            setBoardRect(rect: DOMRect): void {
                set(() => ({ boardRect: rect }));
            },
        })),
        shallow,
    );
}
