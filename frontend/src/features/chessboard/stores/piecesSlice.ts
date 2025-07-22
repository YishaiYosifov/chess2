import { Move, PieceID, PieceMap, Point } from "@/types/tempModels";
import type { ChessboardState } from "./chessboardStore";
import { StateCreator } from "zustand";
import { pointEquals, pointToStr } from "@/lib/utils/pointUtils";

export interface PieceSliceProps {
    pieces: PieceMap;
    onPieceMovement?: (from: Point, to: Point) => Promise<void>;
}

export interface PiecesSlice {
    pieces: PieceMap;
    animatingPieces: Set<PieceID>;
    selectedPieceId: PieceID | null;

    onPieceMovement?: (from: Point, to: Point) => Promise<void>;

    selectPiece(piece: PieceID): void;
    moveSelectedPiece(to: Point): Promise<boolean>;
    moveSelectedPieceToMouse(mousePoint: Point): Promise<boolean>;
    applyMove(move: Move): void;
    updatePiecePosition(from: Point, to: Point): void;

    addAnimatingPiece(pieceId: PieceID): void;
    pointToPiece(position: Point): PieceID | undefined;
    screenPointToPiece(position: Point): PieceID | undefined;
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

        selectedPieceId: null,
        animatingPieces: new Set(),

        selectPiece(piece: PieceID): void {
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
        applyMove(move: Move): void {
            const { pointToPiece, updatePiecePosition } = get();

            const captureIds = [pointToPiece(move.to)];
            for (const capture of move.captures)
                captureIds.push(pointToPiece(capture));

            set((state) => {
                for (const captureId of captureIds) {
                    if (captureId) state.pieces.delete(captureId);
                }
            });

            updatePiecePosition(move.from, move.to);
            for (const sideEffect of move.sideEffects) {
                updatePiecePosition(sideEffect.from, sideEffect.to);
            }
        },

        updatePiecePosition(from: Point, to: Point) {
            const { pointToPiece, addAnimatingPiece } = get();

            const pieceId = pointToPiece(from);
            if (!pieceId) {
                console.warn(
                    `Could not move piece from ${pointToStr(from)} to ${pointToStr(to)} ` +
                        `because no piece was found at ${pointToStr(from)}`,
                );

                return;
            }
            addAnimatingPiece(pieceId);

            set((state) => {
                state.pieces.get(pieceId)!.position = to;
            });
        },

        /**
         * Attempts to move the currently selected piece to a new position, if the move is legal.
         * Calls the onPieceMovement callback if defined, then applies the move and clears selection.
         *
         * @param to - The target position to move the selected piece to.
         */
        async moveSelectedPiece(to: Point): Promise<boolean> {
            const strTo = pointToStr(to);

            const {
                selectedPieceId,
                onPieceMovement,
                applyMove: playMove,
                legalMoves,
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

            const moves = legalMoves.get(strFrom);
            if (!moves) return false;

            const move = moves?.find(
                (candidateMove) =>
                    pointEquals(candidateMove.to, to) ||
                    candidateMove.triggers.some((triggerPoint) =>
                        pointEquals(triggerPoint, to),
                    ),
            );
            if (!move) return false;

            await onPieceMovement?.(from, move.to);
            playMove(move);

            set((state) => {
                state.legalMoves = new Map();
                state.highlightedLegalMoves = [];
                state.selectedPieceId = null;
            });
            return true;
        },

        /**
         * Handles a piece drop event based on mouse coordinates relative to the board.
         * Converts pixel coordinates to board position, accounting for viewing orientation,
         * then attempts to move the selected piece to that position.
         *
         * @param mouseX - The x-coordinate of the mouse event relative to the viewport.
         * @param mouseY - The y-coordinate of the mouse event relative to the viewport.
         */
        async moveSelectedPieceToMouse(mousePoint: Point): Promise<boolean> {
            const { moveSelectedPiece, screenToLogicalPoint } = get();

            const logicalPoint = screenToLogicalPoint({
                x: mousePoint.x,
                y: mousePoint.y,
            });
            if (!logicalPoint) return false;

            const didMove = await moveSelectedPiece(logicalPoint);
            return didMove;
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
         * Returns the ID of the piece located at a given board position.
         *
         * @param position - The board coordinate to check for a piece.
         * @returns The ID of the piece at the position, or undefined if no piece is present.
         */
        pointToPiece(position: Point): PieceID | undefined {
            const pieces = get().pieces;
            for (const [id, piece] of pieces) {
                if (pointEquals(piece.position, position)) return id;
            }
        },

        screenPointToPiece(point: Point): PieceID | undefined {
            const { screenToLogicalPoint, pointToPiece } = get();

            const logicalPoint = screenToLogicalPoint(point);
            if (!logicalPoint) return;

            return pointToPiece(logicalPoint);
        },
    });
}
