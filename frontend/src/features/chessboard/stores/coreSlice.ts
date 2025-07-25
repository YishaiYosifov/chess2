import type { ChessboardProps, ChessboardState } from "./chessboardStore";
import { StateCreator } from "zustand";
import { createMoveOptions } from "../lib/moveOptions";

export interface CoreSlice {
    resetState(initState: ChessboardProps): void;
    disableMovement(): void;
}

export const createCoreSlice: StateCreator<
    ChessboardState,
    [["zustand/immer", never], never],
    [],
    CoreSlice
> = (set, _, store) => ({
    /**
     * Resets the entire chessboard state to defaults, then sets
     * the provided pieces and legal moves
     *
     * @param pieces - The new piece map for the board.
     * @param legalMoves - The new legal moves map.
     */
    resetState(initState) {
        set(() => ({
            ...store.getInitialState(),
            ...initState,
        }));
    },

    disableMovement(): void {
        set((state) => {
            state.moveOptions = createMoveOptions();
            state.highlightedLegalMoves = [];
            state.selectedPieceId = null;
        });
    },
});
