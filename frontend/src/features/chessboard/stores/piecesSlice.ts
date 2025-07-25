import {
    BoardState,
    LogicalPoint,
    Move,
    PieceID,
    PieceMap,
    ScreenPoint,
} from "@/types/tempModels";
import type { ChessboardState } from "./chessboardStore";
import { StateCreator } from "zustand";
import { pointEquals, pointToStr } from "@/lib/utils/pointUtils";
import { pointToPiece, simulateMove } from "../lib/simulateMove";
import { createMoveOptions } from "../lib/moveOptions";

export interface PieceSliceProps {
    pieces: PieceMap;
    onPieceMovement?: (from: LogicalPoint, to: LogicalPoint) => Promise<void>;
}

export interface PiecesSlice {
    pieces: PieceMap;
    piecesCache: Map<number, PieceMap>;
    animatingPieces: Set<PieceID>;
    selectedPieceId: PieceID | null;

    onPieceMovement?: (from: LogicalPoint, to: LogicalPoint) => Promise<void>;

    selectPiece(piece: PieceID): void;
    tryApplySelectedMove(to: LogicalPoint): Promise<boolean>;
    handleMousePieceDrop({
        mousePoint,
        isDrag,
    }: {
        mousePoint: ScreenPoint;
        isDrag: boolean;
    }): Promise<boolean>;
    applyMove(move: Move): void;
    setPosition(boardSnapshot: BoardState): void;

    addAnimatingPiece(pieceId: PieceID): void;
    screenPointToPiece(position: ScreenPoint): PieceID | undefined;
}

export function createPiecesSlice(
    initState: PieceSliceProps,
): StateCreator<
    ChessboardState,
    [["zustand/immer", never], never],
    [],
    PiecesSlice
> {
    return (set, get) => ({
        ...initState,

        piecesCache: new Map([[0, initState.pieces]]),
        selectedPieceId: null,
        animatingPieces: new Set(),

        selectPiece(piece) {
            const { showLegalMoves } = get();

            showLegalMoves(piece);
            set((state) => {
                state.selectedPieceId = piece;
            });
        },

        /**
         * Applies a move on the board by updating piece positions and removing captured pieces.
         * Animates the moving piece, and recursively applies any side effects of the move.
         *
         * @param move - The move to apply on the board.
         */
        applyMove(move) {
            const { addAnimatingPiece, pieces } = get();
            const { newPieces, movedPieceIds } = simulateMove(pieces, move);

            movedPieceIds.forEach(addAnimatingPiece);
            set((state) => {
                state.pieces = newPieces;
            });
        },

        /**
         * Attempts to move the currently selected piece to a new position, if the move is legal.
         * Calls the onPieceMovement callback if defined, then applies the move and clears selection.
         *
         * @param to - The target position to move the selected piece to.
         */
        async tryApplySelectedMove(to) {
            const strTo = pointToStr(to);

            const {
                selectedPieceId,
                onPieceMovement,
                applyMove,
                moveOptions,
                pieces,
            } = get();
            if (!selectedPieceId) {
                console.warn(
                    `Could not execute piece movement to ${strTo} ` +
                        "because no piece was selected",
                );
                return false;
            }

            const selectedPiece = pieces.get(selectedPieceId)!;
            const from = selectedPiece.position;
            const strFrom = pointToStr(from);

            const moves = moveOptions.legalMoves.get(strFrom);
            if (!moves) return false;

            const move = moves?.find(
                (candidateMove) =>
                    pointEquals(candidateMove.to, to) ||
                    candidateMove.triggers.some((triggerPoint) =>
                        pointEquals(triggerPoint, to),
                    ),
            );
            if (!move) return false;

            applyMove(move);
            await onPieceMovement?.(from, move.to);

            set((state) => {
                state.moveOptions = createMoveOptions();
                state.highlightedLegalMoves = [];
                state.selectedPieceId = null;
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

            const didMove = await tryApplySelectedMove(logicalPoint);
            if (isDrag && !didMove && moveOptions.hasForcedMoves)
                flashLegalMoves();

            return didMove;
        },

        setPosition(boardState) {
            const { addAnimatingPiece, pieces } = get();
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
                state.moveOptions = boardState.moveOptions;
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

            setTimeout(
                () =>
                    set((state) => {
                        state.animatingPieces.delete(pieceId);
                    }),
                100,
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
