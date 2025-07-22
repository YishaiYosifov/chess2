import { StateCreator } from "zustand";
import type { ChessboardState } from "./chessboardStore";
import { Point } from "@/types/tempModels";
import { pointToStr } from "@/lib/utils/pointUtils";

export interface OverlayItem {
    from: Point;
    to: Point;
    color?: string;
}

export interface OverlaySlice {
    overlays: Map<string, OverlayItem>;
    currentlyDrawing: OverlayItem | null;

    setCurrentlyDrawing: (from: Point, to: Point) => void;
    commitCurrentlyDrawing: () => void;

    clearOverlays: () => void;
}

export const createOverlaySlice: StateCreator<
    ChessboardState,
    [["zustand/immer", never], never],
    [],
    OverlaySlice
> = (set, get) => ({
    overlays: new Map(),
    currentlyDrawing: null,

    setCurrentlyDrawing: (from, to) =>
        set((state) => {
            state.currentlyDrawing = { from, to };
        }),
    commitCurrentlyDrawing() {
        const { currentlyDrawing, overlays } = get();
        if (!currentlyDrawing) return;

        const id = `${pointToStr(currentlyDrawing.from)}-${pointToStr(currentlyDrawing.to)}`;
        if (overlays.has(id)) {
            set((state) => {
                state.currentlyDrawing = null;
                state.overlays.delete(id);
            });
            return;
        }

        set((state) => {
            state.overlays.set(id, currentlyDrawing);
            state.currentlyDrawing = null;
        });
    },

    clearOverlays: () =>
        set((state) => {
            state.overlays = new Map();
            state.currentlyDrawing = null;
        }),
});
