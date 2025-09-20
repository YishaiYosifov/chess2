import { LogicalPoint } from "@/features/point/types";
import { ScreenPoint } from "@/features/point/types";
import { BoardState, PieceID } from "../lib/types";
import { Move } from "../lib/types";
import { PieceMap } from "../lib/types";
import { MoveKey } from "../lib/types";
import type { ChessboardStore } from "./chessboardStore";
import { StateCreator } from "zustand";
import { pointEquals, pointToStr } from "@/lib/utils/pointUtils";
import {
    pointToPiece,
    simulateMove,
    simulateMoveWithIntermediates,
} from "../lib/simulateMove";

export interface PieceSliceProps {
    pieces: PieceMap;
    onPieceMovement?: (key: MoveKey) => Promise<void>;
}

export interface PiecesSlice {
    pieces: PieceMap;
    intermediatePieces: PieceMap | null;
    animatingPieces: Set<PieceID>;
    selectedPieceId: PieceID | null;

    onPieceMovement?: (key: MoveKey) => Promise<void>;

    selectPiece(piece: PieceID): void;
    tryApplySelectedMove({
        dest,
        isDrag,
    }: {
        dest: LogicalPoint;
        isDrag: boolean;
    }): Promise<boolean>;
    handleMousePieceDrop({
        mousePoint,
        isDrag,
    }: {
        mousePoint: ScreenPoint;
        isDrag: boolean;
    }): Promise<boolean>;

    applyMove(move: Move): void;
    applyMoveWithIntermediates(move: Move): Promise<void>;
    goToPosition(
        boardState: BoardState,
        options?: { animateIntermediates?: boolean },
    ): Promise<void>;

    addAnimatingPiece(pieceId: PieceID): Promise<void>;
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

        intermediatePieces: null,
        selectedPieceId: null,
        animatingPieces: new Set(),

        selectPiece(piece) {
            const { showLegalMoves } = get();

            showLegalMoves(piece);
            set((state) => {
                state.selectedPieceId = piece;
            });
        },

        async applyMoveWithIntermediates(move) {
            const { addAnimatingPiece, pieces } = get();

            const positions = simulateMoveWithIntermediates(pieces, move);
            const lastPosition = positions[positions.length - 1];
            set((state) => {
                state.pieces = lastPosition.newPieces;
            });

            for (const { movedPieceIds, newPieces } of positions) {
                const animatingPromises = movedPieceIds
                    .values()
                    .map(addAnimatingPiece);
                set((state) => {
                    state.intermediatePieces = newPieces;
                });
                await Promise.all(animatingPromises);
            }
            set((state) => {
                state.intermediatePieces = null;
            });
        },

        applyMove(move) {
            const { addAnimatingPiece, pieces } = get();
            const { newPieces, movedPieceIds } = simulateMove(pieces, move);

            movedPieceIds.forEach(addAnimatingPiece);
            set((state) => {
                state.pieces = newPieces;
            });
        },

        async tryApplySelectedMove({ dest, isDrag }) {
            const {
                selectedPieceId,
                getLegalMove,
                onPieceMovement,
                applyMove,
                applyMoveWithIntermediates,
                pieces,
                disableMovement,
            } = get();
            if (!selectedPieceId) {
                console.warn(
                    `Could not execute piece movement to ${pointToStr(dest)} ` +
                        "because no piece was selected",
                );
                return false;
            }

            const selectedPiece = pieces.get(selectedPieceId)!;
            const move = await getLegalMove(
                selectedPiece.position,
                dest,
                selectedPiece,
            );
            if (!move) return false;

            if (isDrag) applyMove(move);
            else applyMoveWithIntermediates(move);

            disableMovement();
            await onPieceMovement?.({
                from: move.from,
                to: move.to,
                promotesTo: move.promotesTo,
            });

            return true;
        },

        /**
         * Handles a piece drop event based on mouse coordinates relative to the board.
         * Converts pixel coordinates to board position, accounting for viewing orientation,
         * then attempts to move the selected piece to that position.
         */
        async handleMousePieceDrop({ mousePoint, isDrag }) {
            const {
                tryApplySelectedMove,
                screenToLogicalPoint,
                flashLegalMoves,
                moveOptions,
            } = get();

            const logicalPoint = screenToLogicalPoint(mousePoint);
            if (!logicalPoint) return false;

            const didMove = await tryApplySelectedMove({
                dest: logicalPoint,
                isDrag,
            });

            const shouldFlashLegalMoves =
                moveOptions.hasForcedMoves &&
                isDrag && // player tried to phyically move the piece, not just click and click somewhere else
                !didMove;
            if (shouldFlashLegalMoves) flashLegalMoves();

            return didMove;
        },

        async goToPosition(boardState, options) {
            const {
                addAnimatingPiece,
                applyMoveWithIntermediates,
                setLegalMoves,
                pieces,
            } = get();
            setLegalMoves(boardState.moveOptions);

            if (options?.animateIntermediates && boardState.casuedByMove) {
                await applyMoveWithIntermediates(boardState.casuedByMove);
                return;
            }

            const movedPieces: PieceID[] = [];
            for (const [id, newPiece] of boardState.pieces) {
                const piece = pieces.get(id);
                if (!piece) continue;
                if (!pointEquals(piece.position, newPiece.position))
                    movedPieces.push(id);
            }

            movedPieces.forEach(addAnimatingPiece);
            set((state) => {
                state.pieces = boardState.pieces;
                state.highlightedLegalMoves = [];
                state.selectedPieceId = null;
            });
        },

        /**
         * Adds a piece ID to the set of currently animating pieces,
         * then removes it after a short delay to control animation lifecycle.
         *
         * @param pieceId - The ID of the piece to animate.
         */
        addAnimatingPiece(pieceId) {
            set((state) => {
                if (!state.animatingPieces.has(pieceId))
                    state.animatingPieces.add(pieceId);
            });

            return new Promise((resolve) =>
                setTimeout(() => {
                    set((state) => {
                        state.animatingPieces.delete(pieceId);
                    });
                    resolve();
                }, 100),
            );
        },

        screenPointToPiece(point) {
            const { screenToLogicalPoint, pieces } = get();

            const logicalPoint = screenToLogicalPoint(point);
            if (!logicalPoint) return;

            return pointToPiece(pieces, logicalPoint);
        },
    });
}
