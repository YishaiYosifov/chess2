import { LogicalPoint } from "@/features/point/types";
import { ScreenPoint } from "@/features/point/types";
import { BoardState, MoveBounds, PieceID } from "../lib/types";
import { Move } from "../lib/types";
import type { ChessboardStore } from "./chessboardStore";
import { StateCreator } from "zustand";
import { pointEquals } from "@/features/point/pointUtils";
import {
    simulateMove,
    simulateMoveWithIntermediates,
} from "../lib/simulateMove";
import BoardPieces from "../lib/boardPieces";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";

export interface PieceSliceProps {
    pieces: BoardPieces;
    canDrag: boolean;
    onPieceMovement?: (move: Move) => Promise<void>;
}

export interface PiecesSlice {
    pieces: BoardPieces;
    selectedPieceId: PieceID | null;
    canDrag: boolean;
    isProcessingMove: boolean;

    onPieceMovement?: (move: Move) => Promise<void>;

    selectPiece(pieceId: PieceID): boolean;
    unselectPiece(): void;

    handleMousePieceDrop(args: {
        mousePoint: ScreenPoint;
        isDrag: boolean;
        isDoubleClick: boolean;
    }): Promise<{ success: boolean; needsDoubleClick?: boolean }>;
    applyMoveAnimated(move: Move): Promise<void>;
    applyMoveImmediate(move: Move): Promise<void>;

    goToPosition(
        boardState: BoardState,
        options?: { animateIntermediates?: boolean },
    ): Promise<void>;

    screenPointToPiece(position: ScreenPoint): PieceID | undefined;
}

export function createPiecesSlice(
    initState: PieceSliceProps,
): StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    PiecesSlice
> {
    return (set, get) => {
        async function applyMoveTurn(move: Move): Promise<void> {
            const { applyMoveImmediate, disableMovement, onPieceMovement } =
                get();

            const animationPromise = applyMoveImmediate(move);
            disableMovement();
            await onPieceMovement?.(move);
            await animationPromise;
        }

        function detectNeedsDoubleClick(dest: LogicalPoint): boolean {
            const { selectedPieceId, pieces, hasMovesFromTo } = get();
            if (!selectedPieceId) return false;

            const piece = pieces.getById(selectedPieceId);
            if (!piece) return false;

            return (
                pointEquals(piece.position, dest) &&
                hasMovesFromTo(piece.position, dest)
            );
        }

        async function getMoveForSelection(
            dest: LogicalPoint,
        ): Promise<Move | null> {
            const { selectedPieceId, getLegalMove, pieces } = get();
            if (!selectedPieceId) return null;

            const move = await getLegalMove(dest, selectedPieceId, pieces);
            return move;
        }

        function findMovedPiecesBetween(
            oldPieces: BoardPieces,
            newPieces: BoardPieces,
        ): PieceID[] {
            const movedPieceIds: PieceID[] = [];
            for (const newPiece of oldPieces) {
                const piece = newPieces.getById(newPiece.id);
                if (!piece) continue;
                if (!pointEquals(piece.position, newPiece.position))
                    movedPieceIds.push(newPiece.id);
            }

            return movedPieceIds;
        }

        return {
            ...initState,

            selectedPieceId: null,
            animatingPieces: new Set(),
            isProcessingMove: false,

            selectPiece(pieceId) {
                const { showLegalMoves, pieces, selectedPieceId } = get();
                const piece = pieces.getById(pieceId);
                if (!piece) {
                    console.warn(
                        `Cannot show legal moves, no piece was found with id ${pieceId}`,
                    );
                    return false;
                }
                if (pieceId === selectedPieceId) return false;

                const hasLegalMoves = showLegalMoves(piece);
                set((state) => {
                    state.selectedPieceId = hasLegalMoves ? pieceId : null;
                });

                return hasLegalMoves;
            },
            unselectPiece() {
                const { hideLegalMoves } = get();

                hideLegalMoves();
                set((state) => {
                    state.selectedPieceId = null;
                });
            },

            async applyMoveImmediate(move: Move): Promise<void> {
                const { playAnimation, pieces } = get();
                const animation = simulateMove(pieces, move);

                set((state) => {
                    state.pieces = animation.newPieces;
                });
                await playAnimation(animation);
            },

            async applyMoveAnimated(move: Move): Promise<void> {
                const { playAnimationBatch, pieces } = get();

                const positions = simulateMoveWithIntermediates(pieces, move);
                const lastPosition = positions.steps.at(-1);
                if (!lastPosition) return;

                set((state) => {
                    state.pieces = lastPosition.newPieces;
                });
                await playAnimationBatch(positions);
            },

            async handleMousePieceDrop({ mousePoint, isDrag, isDoubleClick }) {
                const {
                    screenToLogicalPoint,
                    flashLegalMoves,
                    clearAnimation,
                    moveOptions,
                    isProcessingMove,
                } = get();
                if (isProcessingMove) return { success: false };

                set((state) => {
                    state.isProcessingMove = true;
                });
                try {
                    const dest = screenToLogicalPoint(mousePoint);
                    if (!dest) return { success: false };

                    const needsDoubleClick = detectNeedsDoubleClick(dest);
                    if (needsDoubleClick && !isDoubleClick)
                        return { success: false, needsDoubleClick: true };

                    const move = await getMoveForSelection(dest);
                    if (move) {
                        await applyMoveTurn(move);
                        return { success: true };
                    }
                    clearAnimation();

                    if (
                        moveOptions.hasForcedMoves &&
                        isDrag // player tried to phyically move the piece, not just click and click somewhere else
                    ) {
                        flashLegalMoves();
                        AudioPlayer.playAudio(AudioType.ILLEGAL_MOVE);
                    }

                    return { success: false };
                } finally {
                    set((state) => {
                        state.isProcessingMove = false;
                    });
                }
            },

            async goToPosition(boardState, options) {
                const {
                    applyMoveAnimated,
                    playAnimation,
                    setLegalMoves,
                    pieces,
                } = get();

                setLegalMoves(boardState.moveOptions);

                const {
                    moveFromPreviousViewedPosition,
                    moveThatProducedPosition,
                } = boardState;

                if (options?.animateIntermediates && moveThatProducedPosition) {
                    await applyMoveAnimated(moveThatProducedPosition);
                    return;
                }

                const movedPieceIds = findMovedPiecesBetween(
                    pieces,
                    boardState.pieces,
                );

                set((state) => {
                    state.pieces = boardState.pieces;
                    state.selectedPieceId = null;
                });

                const moveBounds: MoveBounds | undefined =
                    moveThatProducedPosition
                        ? {
                              from: moveThatProducedPosition.from,
                              to: moveThatProducedPosition.to,
                          }
                        : undefined;
                const isCapture = moveFromPreviousViewedPosition
                    ? moveFromPreviousViewedPosition.captures.length > 0
                    : false;
                const isPromotion = moveFromPreviousViewedPosition
                    ? moveFromPreviousViewedPosition.promotesTo !== null
                    : false;

                await playAnimation({
                    newPieces: boardState.pieces,
                    movedPieceIds,

                    moveBounds,
                    isCapture,
                    isPromotion,
                    specialMoveType:
                        moveFromPreviousViewedPosition?.specialMoveType,
                });
            },

            screenPointToPiece(point) {
                const { screenToLogicalPoint, pieces } = get();

                const logicalPoint = screenToLogicalPoint(point);
                if (!logicalPoint) return;

                return pieces.getByPosition(logicalPoint)?.id;
            },
        };
    };
}
