import { StateCreator } from "zustand";
import type { ChessboardState } from "./chessboardStore";
import { Point } from "@/types/tempModels";

export interface OverlayItem {
    from: Point;
    to: Point;
}

export interface OverlaySlice {
    overlays: OverlayItem[];
    currentlyDrawing?: OverlayItem;

    addOverlay: (from: Point, to: Point) => void;

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
    overlays: [],

    addOverlay: (from, to) =>
        set((state) => {
            state.overlays.push({ from, to });
        }),

    setCurrentlyDrawing: (from, to) =>
        set((state) => {
            state.currentlyDrawing = { from, to };
        }),
    commitCurrentlyDrawing() {
        const { currentlyDrawing, addOverlay } = get();
        if (!currentlyDrawing) return;

        addOverlay(currentlyDrawing.from, currentlyDrawing.to);
        set((state) => {
            state.currentlyDrawing = undefined;
        });
    },

    clearOverlays: () =>
        set((state) => {
            state.overlays = [];
            state.currentlyDrawing = undefined;
        }),
});
