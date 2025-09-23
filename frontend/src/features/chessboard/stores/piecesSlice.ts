import { LogicalPoint } from "@/features/point/types";
import { ScreenPoint } from "@/features/point/types";
import { BoardState, PieceID } from "../lib/types";
import { Move } from "../lib/types";
import { PieceMap } from "../lib/types";
import type { ChessboardStore } from "./chessboardStore";
import { StateCreator } from "zustand";
import { pointEquals, pointToStr } from "@/features/point/pointUtils";
import {
    pointToPiece,
    simulateMove,
    simulateMoveWithIntermediates,
} from "../lib/simulateMove";

export interface PieceSliceProps {
    pieceMap: PieceMap;
    onPieceMovement?: (move: Move) => Promise<void>;
}

export interface PiecesSlice {
    pieceMap: PieceMap;
    selectedPieceId: PieceID | null;

    onPieceMovement?: (move: Move) => Promise<void>;

    selectPiece(pieceId: PieceID): void;
    getMoveForSelection(dest: LogicalPoint): Promise<Move | null>;

    applyMoveTurn(move: Move): Promise<void>;
    handleMousePieceDrop({
        mousePoint,
        isDrag,
    }: {
        mousePoint: ScreenPoint;
        isDrag: boolean;
    }): Promise<boolean>;

    applyMove(move: Move): Promise<void>;
    applyMoveWithIntermediates(move: Move): Promise<void>;
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
    return (set, get) => ({
        ...initState,

        animatingPieceMap: null,
        selectedPieceId: null,
        animatingPieces: new Set(),

        selectPiece(pieceId) {
            const { showLegalMoves, pieceMap } = get();
            const piece = pieceMap.get(pieceId);
            if (!piece) {
                console.warn(
                    `Cannot show legal moves, no piece was found with id ${pieceId}`,
                );
                return;
            }

            showLegalMoves(piece);
            set((state) => {
                state.selectedPieceId = pieceId;
            });
        },

        async applyMoveWithIntermediates(move) {
            const { playAnimationBatch, pieceMap } = get();

            const positions = simulateMoveWithIntermediates(pieceMap, move);
            const lastPosition = positions.at(-1);
            if (!lastPosition) return;

            set((state) => {
                state.pieceMap = lastPosition.newPieces;
            });
            await playAnimationBatch(positions);
        },

        async applyMove(move) {
            const { playAnimationBatch, pieceMap } = get();
            const { newPieces, movedPieceIds } = simulateMove(pieceMap, move);

            set((state) => {
                state.pieceMap = newPieces;
            });
            await playAnimationBatch([{ newPieces, movedPieceIds }]);
        },

        async applyMoveTurn(move) {
            const { applyMove, disableMovement, onPieceMovement } = get();

            applyMove(move);
            disableMovement();
            await onPieceMovement?.(move);
        },

        /**
         * Handles a piece drop event based on mouse coordinates relative to the board.
         * Converts pixel coordinates to board position, accounting for viewing orientation,
         * then attempts to move the selected piece to that position.
         */
        async handleMousePieceDrop({ mousePoint, isDrag }) {
            const {
                applyMoveTurn,
                screenToLogicalPoint,
                flashLegalMoves,
                getMoveForSelection,
                moveOptions,
            } = get();

            const dest = screenToLogicalPoint(mousePoint);
            if (!dest) return false;

            const move = await getMoveForSelection(dest);
            if (move) {
                await applyMoveTurn(move);
                return true;
            }

            if (
                moveOptions.hasForcedMoves &&
                isDrag // player tried to phyically move the piece, not just click and click somewhere else
            ) {
                flashLegalMoves();
            }

            return false;
        },

        async getMoveForSelection(dest) {
            const { selectedPieceId, getLegalMove, pieceMap } = get();
            if (!selectedPieceId) {
                console.warn(
                    `Could not execute piece movement to ${pointToStr(dest)} ` +
                        "because no piece was selected",
                );
                return null;
            }

            const selectedPiece = pieceMap.get(selectedPieceId)!;
            const move = await getLegalMove(
                selectedPiece.position,
                dest,
                selectedPieceId,
                selectedPiece,
            );
            return move;
        },

        async goToPosition(boardState, options) {
            const {
                playAnimationBatch,
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
            await playAnimationBatch([
                { newPieces: boardState.pieces, movedPieceIds: movedPieces },
            ]);
        },

        screenPointToPiece(point) {
            const { screenToLogicalPoint, pieceMap } = get();

            const logicalPoint = screenToLogicalPoint(point);
            if (!logicalPoint) return;

            return pointToPiece(pieceMap, logicalPoint);
        },
    });
}
