import type { ChessboardProps, ChessboardStore } from "./chessboardStore";
import { StateCreator } from "zustand";
import { createMoveOptions } from "../lib/moveOptions";

export interface CoreSlice {
    resetState(initState: ChessboardProps): void;
    disableMovement(): void;
}

export const createCoreSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    CoreSlice
> = (set, _, store) => ({
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
