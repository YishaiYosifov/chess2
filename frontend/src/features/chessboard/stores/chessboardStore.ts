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

    boardDimensions: BoardDimensions;
    boardRect?: DOMRect;

    pieces: PieceMap;
    legalMoves: LegalMoveMap;

    highlightedLegalMoves: Point[];
    animatingPieces: Set<PieceID>;

    selectedPieceId?: PieceID;

    onPieceMovement?: (from: Point, to: Point) => Promise<void>;

    playMove(move: Move): void;
    moveSelectedPiece(to: Point): Promise<void>;
    handlePieceDrop(mouseX: number, mouseY: number): Promise<void>;
    position2Id(position: Point): PieceID | undefined;
    showLegalMoves(pieceId: PieceID): void;
    addAnimatingPiece(pieceId: PieceID): void;
    setBoardRect(rect: DOMRect): void;
    resetState(
        pieces: PieceMap,
        legalMoves: LegalMoveMap,
        sideToMove: GameColor,
    ): void;
    setLegalMoves(legalMoves: LegalMoveMap): void;
    disableMovement(): void;
}

export const defaultChessboardState = {
    viewingFrom: GameColor.WHITE,

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

    highlightedLegalMoves: [],
    animatingPieces: new Set<PieceID>(),
};

enableMapSet();
export function createChessboardStore(
    initState: Partial<ChessboardStore> = {},
) {
    return createWithEqualityFn<ChessboardStore>()(
        immer((set, get) => ({
            ...defaultChessboardState,
            ...initState,

            /**
             * Applies a move on the board by updating piece positions and removing captured pieces.
             * Animates the moving piece, and recursively applies any side effects of the move.
             *
             * @param move - The move to apply on the board.
             */
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
             * Attempts to move the currently selected piece to a new position, if the move is legal.
             * Calls the onPieceMovement callback if defined, then applies the move and clears selection.
             *
             * @param to - The target position to move the selected piece to.
             */
            async moveSelectedPiece(to: Point): Promise<void> {
                const strTo = pointToStr(to);

                const {
                    selectedPieceId,
                    onPieceMovement,
                    playMove,
                    legalMoves,
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
                if (!moves || moves.length === 0) return;

                const move = moves?.find(
                    (m) =>
                        (m.to.x == to.x && m.to.y == to.y) ||
                        m.triggers.includes(to),
                );
                if (!move) return;

                await onPieceMovement?.(from, move.to);
                playMove(move);

                set((state) => {
                    state.legalMoves = new Map();
                    state.highlightedLegalMoves = [];
                    state.selectedPieceId = undefined;
                });
            },

            /**
             * Handles a piece drop event based on mouse coordinates relative to the board.
             * Converts pixel coordinates to board position, accounting for viewing orientation,
             * then attempts to move the selected piece to that position.
             *
             * @param mouseX - The x-coordinate of the mouse event relative to the viewport.
             * @param mouseY - The y-coordinate of the mouse event relative to the viewport.
             */
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
             * Returns the ID of the piece located at a given board position.
             *
             * @param position - The board coordinate to check for a piece.
             * @returns The ID of the piece at the position, or undefined if no piece is present.
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
             * Highlights the legal moves available for the specified piece.
             * Updates the state to reflect these highlighted moves and sets the selected piece.
             *
             * @param pieceId - The ID of the piece for which to show legal moves.
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
                          ...moves.map((m) => m.triggers).flat(),
                      ]
                    : [];

                set((state) => {
                    state.highlightedLegalMoves = toHighlightPoints;
                    state.selectedPieceId = pieceId;
                });
            },

            /**
             * Adds a piece ID to the set of currently animating pieces,
             * then removes it after a short delay to control animation lifecycle.
             *
             * @param pieceId - The ID of the piece to animate.
             */
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

            /**
             * Sets the bounding rectangle of the board in screen coordinates.
             * Useful for translating mouse coordinates to board positions.
             *
             * @param rect - The DOMRect representing the board's position and size.
             */
            setBoardRect(rect: DOMRect): void {
                set(() => ({ boardRect: rect }));
            },

            /**
             * Resets the entire chessboard state to defaults, then sets
             * the provided pieces, legal moves, and side to move.
             *
             * @param pieces - The new piece map for the board.
             * @param legalMoves - The new legal moves map.
             * @param sideToMove - The player color to move next.
             */
            resetState(
                pieces: PieceMap,
                legalMoves: LegalMoveMap,
                sideToMove: GameColor,
            ) {
                set(() => ({
                    ...defaultChessboardState,
                    ...initState,

                    pieces,
                    legalMoves,
                    sideToMove,
                }));
            },

            setLegalMoves(legalMoves: LegalMoveMap) {
                set((state) => {
                    state.legalMoves = legalMoves;
                });
            },

            disableMovement(): void {
                set((state) => {
                    state.legalMoves = new Map();
                    state.highlightedLegalMoves = [];
                    state.selectedPieceId = undefined;
                });
            },
        })),
        shallow,
    );
}
