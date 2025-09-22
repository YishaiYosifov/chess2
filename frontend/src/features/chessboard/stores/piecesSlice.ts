import { LogicalPoint } from "@/features/point/types";
import { ScreenPoint } from "@/features/point/types";
import { BoardState, PieceID } from "../lib/types";
import { Move } from "../lib/types";
import { PieceMap } from "../lib/types";
import { MoveKey } from "../lib/types";
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
    onPieceMovement?: (key: MoveKey) => Promise<void>;
}

export interface PiecesSlice {
    pieceMap: PieceMap;
    selectedPieceId: PieceID | null;

    onPieceMovement?: (key: MoveKey) => Promise<void>;

    selectPiece(pieceId: PieceID): void;
    tryApplySelectedMove(dest: LogicalPoint): Promise<boolean>;
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
            const { setAnimatingPieceMap, pieceMap } = get();

            const positions = simulateMoveWithIntermediates(pieceMap, move);
            const lastPosition = positions[positions.length - 1];
            set((state) => {
                state.pieceMap = lastPosition.newPieces;
            });

            for (const { movedPieceIds, newPieces } of positions) {
                await setAnimatingPieceMap(newPieces, movedPieceIds);
            }
            set((state) => {
                state.animatingPieceMap = null;
            });
        },

        applyMove(move) {
            const { addAnimatingPiece, pieceMap } = get();
            const { newPieces, movedPieceIds } = simulateMove(pieceMap, move);

            movedPieceIds.forEach(addAnimatingPiece);
            set((state) => {
                state.pieceMap = newPieces;
            });
        },

        async tryApplySelectedMove(dest) {
            const {
                selectedPieceId,
                getLegalMove,
                onPieceMovement,
                applyMove,
                pieceMap,
                disableMovement,
            } = get();
            if (!selectedPieceId) {
                console.warn(
                    `Could not execute piece movement to ${pointToStr(dest)} ` +
                        "because no piece was selected",
                );
                return false;
            }

            const selectedPiece = pieceMap.get(selectedPieceId)!;
            const move = await getLegalMove(
                selectedPiece.position,
                dest,
                selectedPieceId,
                selectedPiece,
            );
            if (!move) return false;

            applyMove(move);

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

            const dest = screenToLogicalPoint(mousePoint);
            if (!dest) return false;

            const didMove = await tryApplySelectedMove(dest);
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

            movedPieces.forEach(addAnimatingPiece);
            set((state) => {
                state.pieceMap = boardState.pieces;
                state.highlightedLegalMoves = [];
                state.selectedPieceId = null;
            });
        },

        screenPointToPiece(point) {
            const { screenToLogicalPoint, pieceMap } = get();

            const logicalPoint = screenToLogicalPoint(point);
            if (!logicalPoint) return;

            return pointToPiece(pieceMap, logicalPoint);
        },
    });
}
