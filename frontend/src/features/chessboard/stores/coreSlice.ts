import { LegalMoveMap, PieceMap } from "@/types/tempModels";
import type { ChessboardState } from "./chessboardStore";
import { StateCreator } from "zustand";

export interface CoreSlice {
    resetState(pieces: PieceMap, legalMoves: LegalMoveMap): void;
    disableMovement(): void;
}

export const createCoreSlice: StateCreator<
    ChessboardState,
    [["zustand/immer", never], never],
    [],
    CoreSlice
> = (set) => ({
    /**
     * Resets the entire chessboard state to defaults, then sets
     * the provided pieces and legal moves
     *
     * @param pieces - The new piece map for the board.
     * @param legalMoves - The new legal moves map.
     */
    resetState(pieces: PieceMap, legalMoves: LegalMoveMap) {
        set((state) => {
            state.pieces = pieces;
            state.legalMoves = legalMoves;
            state.selectedPieceId = undefined;
            state.highlightedLegalMoves = [];
        });
    },

    disableMovement(): void {
        set((state) => {
            state.legalMoves = new Map();
            state.highlightedLegalMoves = [];
            state.selectedPieceId = undefined;
        });
    },
});
