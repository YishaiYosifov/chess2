import { Move, PieceID, PieceMap, Point } from "@/types/tempModels";
import type { ChessboardState } from "./chessboardStore";
import { StateCreator } from "zustand";
import { pointEquals, pointToStr } from "@/lib/utils/pointUtils";
import { GameColor } from "@/lib/apiClient";

export interface PieceSliceProps {
    pieces: PieceMap;
    onPieceMovement?: (from: Point, to: Point) => Promise<void>;
}

export interface PiecesSlice {
    pieces: PieceMap;
    selectedPieceId?: PieceID;
    animatingPieces: Set<PieceID>;
    onPieceMovement?: (from: Point, to: Point) => Promise<void>;

    moveSelectedPiece(to: Point): Promise<void>;
    handlePieceDrop(mouseX: number, mouseY: number): Promise<void>;
    playMove(move: Move): void;
    movePiece(from: Point, to: Point): void;

    addAnimatingPiece(pieceId: PieceID): void;
    pointToPiece(position: Point): PieceID | undefined;
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

        animatingPieces: new Set(),

        /**
         * Applies a move on the board by updating piece positions and removing captured pieces.
         * Animates the moving piece, and recursively applies any side effects of the move.
         *
         * @param move - The move to apply on the board.
         */
        playMove(move: Move): void {
            const { pointToPiece, movePiece } = get();

            const captureIds = [pointToPiece(move.to)];
            for (const capture of move.captures)
                captureIds.push(pointToPiece(capture));

            set((state) => {
                for (const captureId of captureIds) {
                    if (captureId) state.pieces.delete(captureId);
                }
            });

            movePiece(move.from, move.to);
            for (const sideEffect of move.sideEffects) {
                movePiece(sideEffect.from, sideEffect.to);
            }
        },

        movePiece(from: Point, to: Point) {
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
            if (!moves) return;

            const move = moves?.find(
                (candidateMove) =>
                    pointEquals(candidateMove.to, to) ||
                    candidateMove.triggers.some((triggerPoint) =>
                        pointEquals(triggerPoint, to),
                    ),
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
        async handlePieceDrop(mouseX: number, mouseY: number): Promise<void> {
            const { moveSelectedPiece, screenToPiecePoint } = get();

            const piecePoint = screenToPiecePoint({
                x: mouseX,
                y: mouseY,
            });
            if (!piecePoint) return;

            await moveSelectedPiece(piecePoint);
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
    });
}
