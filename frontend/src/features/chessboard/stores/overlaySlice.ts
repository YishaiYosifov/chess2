import { StateCreator } from "zustand";
import type { ChessboardState } from "./chessboardStore";
import { Point, StrPoint } from "@/types/tempModels";
import { pointToStr } from "@/lib/utils/pointUtils";

type OverlayItemId = `${StrPoint}-${StrPoint}`;

export interface OverlayItem {
    from: Point;
    to: Point;
    color?: string;
}

export interface OverlaySlice {
    overlays: Map<OverlayItemId, OverlayItem>;
    currentlyDrawing: OverlayItem | null;

    toggleOverlay(overlay: OverlayItem): void;
    addOverlay(overlay: OverlayItem): void;
    removeOverlay(id: OverlayItemId): void;

    setCurrentlyDrawing(from: Point, to: Point): void;
    commitCurrentlyDrawing(): void;

    getOverlayId(from: Point, to: Point): OverlayItemId;
    clearOverlays(): void;
}

export const createOverlaySlice: StateCreator<
    ChessboardState,
    [["zustand/immer", never], never],
    [],
    OverlaySlice
> = (set, get) => ({
    overlays: new Map(),
    currentlyDrawing: null,

    toggleOverlay(overlay) {
        const { getOverlayId, overlays, addOverlay, removeOverlay } = get();
        const id = getOverlayId(overlay.from, overlay.to);

        if (overlays.has(id)) removeOverlay(id);
        else addOverlay(overlay);
    },

    addOverlay(overlay) {
        const { getOverlayId } = get();

        const id = getOverlayId(overlay.from, overlay.to);
        set((state) => {
            state.overlays.set(id, overlay);
        });
    },
    removeOverlay(id) {
        set((state) => {
            state.overlays.delete(id);
        });
    },

    setCurrentlyDrawing: (from, to) =>
        set((state) => {
            state.currentlyDrawing = { from, to };
        }),
    commitCurrentlyDrawing() {
        const { currentlyDrawing, toggleOverlay } = get();
        if (!currentlyDrawing) return;

        toggleOverlay(currentlyDrawing);
        set((store) => {
            store.currentlyDrawing = null;
        });
    },

    getOverlayId: (from, to) => `${pointToStr(from)}-${pointToStr(to)}`,
    clearOverlays: () =>
        set((state) => {
            state.overlays = new Map();
            state.currentlyDrawing = null;
        }),
});
