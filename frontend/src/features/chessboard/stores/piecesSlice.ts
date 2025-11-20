import { LogicalPoint } from "@/features/point/types";
import { ScreenPoint } from "@/features/point/types";
import { BoardState, PieceID } from "../lib/types";
import { Move } from "../lib/types";
import { PieceMap } from "../lib/types";
import type { ChessboardStore } from "./chessboardStore";
import { StateCreator } from "zustand";
import { pointEquals } from "@/features/point/pointUtils";
import {
    pointToPiece,
    simulateMove,
    simulateMoveWithIntermediates,
} from "../lib/simulateMove";

export interface PieceSliceProps {
    pieceMap: PieceMap;
    canDrag: boolean;
    onPieceMovement?: (move: Move) => Promise<void>;
}

export interface PiecesSlice {
    pieceMap: PieceMap;
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

    applyMove(move: Move): Promise<void>;
    applyMoveWithIntermediates(move: Move): Promise<void>;
    goToPosition(
        boardState: BoardState,
        options?: { animateIntermediates?: boolean },
    ): Promise<void>;

    screenPointToPiece(position: ScreenPoint): PieceID | undefined;
}

export const createPiecesSlice =
    (
        initState: PieceSliceProps,
    ): StateCreator<
        ChessboardStore,
        [["zustand/immer", never], never],
        [],
        PiecesSlice
    > =>
    (set, get) => {
        async function applyMoveTurn(move: Move): Promise<void> {
            const { applyMove, disableMovement, onPieceMovement } = get();

            const animationPromise = applyMove(move);
            disableMovement();
            await onPieceMovement?.(move);
            await animationPromise;
        }

        function detectNeedsDoubleClick(dest: LogicalPoint): boolean {
            const { selectedPieceId, pieceMap, hasMovesFromTo } = get();
            if (!selectedPieceId) return false;

            const piece = pieceMap.get(selectedPieceId);
            if (!piece) return false;

            return (
                pointEquals(piece.position, dest) &&
                hasMovesFromTo(piece.position, dest)
            );
        }

        async function getMoveForSelection(
            dest: LogicalPoint,
        ): Promise<Move | null> {
            const { selectedPieceId, getLegalMove, pieceMap } = get();
            if (!selectedPieceId) return null;

            const move = await getLegalMove(dest, selectedPieceId, pieceMap);
            return move;
        }

        return {
            ...initState,

            animatingPieceMap: null,
            selectedPieceId: null,
            animatingPieces: new Set(),
            isProcessingMove: false,

            selectPiece(pieceId) {
                const { showLegalMoves, pieceMap, selectedPieceId } = get();
                const piece = pieceMap.get(pieceId);
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

            async applyMoveWithIntermediates(move) {
                const { playAnimationBatch, pieceMap } = get();

                const positions = simulateMoveWithIntermediates(pieceMap, move);
                const lastPosition = positions.steps.at(-1);
                if (!lastPosition) return;

                set((state) => {
                    state.pieceMap = lastPosition.newPieces;
                });
                await playAnimationBatch(positions);
            },

            async applyMove(move) {
                const { playAnimation, pieceMap } = get();
                const animation = simulateMove(pieceMap, move);

                set((state) => {
                    state.pieceMap = animation.newPieces;
                });
                await playAnimation(animation);
            },

            /**
             * Handles a piece drop event based on mouse coordinates relative to the board.
             * Converts pixel coordinates to board position, accounting for viewing orientation,
             * then attempts to move the selected piece to that position.
             */
            async handleMousePieceDrop({ mousePoint, isDrag, isDoubleClick }) {
                const {
                    screenToLogicalPoint,
                    flashLegalMoves,
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

                    if (
                        moveOptions.hasForcedMoves &&
                        isDrag // player tried to phyically move the piece, not just click and click somewhere else
                    ) {
                        flashLegalMoves();
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
                    playAnimation,
                    applyMoveWithIntermediates,
                    setLegalMoves,
                    pieceMap,
                } = get();
                setLegalMoves(boardState.moveOptions);

                if (options?.animateIntermediates && boardState.casuedByMove) {
                    await applyMoveWithIntermediates(boardState.casuedByMove);
                    return;
                }

                const movedPieces: PieceID[] = [];
                for (const [id, newPiece] of boardState.pieces) {
                    const piece = pieceMap.get(id);
                    if (!piece) continue;
                    if (!pointEquals(piece.position, newPiece.position))
                        movedPieces.push(id);
                }

                set((state) => {
                    state.pieceMap = boardState.pieces;
                    state.selectedPieceId = null;
                });
                await playAnimation({
                    newPieces: boardState.pieces,
                    movedPieceIds: movedPieces,
                    isCapture: boardState.casuedByMove
                        ? boardState.casuedByMove.captures.length > 0
                        : false,
                });
            },

            screenPointToPiece(point) {
                const { screenToLogicalPoint, pieceMap } = get();

                const logicalPoint = screenToLogicalPoint(point);
                if (!logicalPoint) return;

                return pointToPiece(pieceMap, logicalPoint);
            },
        };
    };
