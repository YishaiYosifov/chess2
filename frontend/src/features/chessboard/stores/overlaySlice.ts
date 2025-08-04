import { StateCreator } from "zustand";
import type { ChessboardStore } from "./chessboardStore";
import { ViewPoint } from "@/features/point/types";
import { StrPoint } from "@/features/point/types";
import { pointToStr } from "@/lib/utils/pointUtils";

type OverlayItemId = `${StrPoint}-${StrPoint}`;

export interface OverlayItem {
    from: ViewPoint;
    to: ViewPoint;
    color?: string;
}

export interface OverlaySlice {
    overlays: Map<OverlayItemId, OverlayItem>;
    currentlyDrawing: OverlayItem | null;
    flashing: Set<OverlayItemId>;

    toggleOverlay(overlay: OverlayItem): void;
    addOverlay(overlay: OverlayItem): void;
    removeOverlay(id: OverlayItemId): void;

    setCurrentlyDrawing(from: ViewPoint, to: ViewPoint): void;
    commitCurrentlyDrawing(): void;

    getOverlayId(from: ViewPoint, to: ViewPoint): OverlayItemId;
    clearOverlays(): void;

    flashOverlay(overlay: OverlayItem, amount?: number): void;
}

export const createOverlaySlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    OverlaySlice
> = (set, get) => ({
    overlays: new Map(),
    currentlyDrawing: null,
    flashing: new Set(),

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

    flashOverlay(overlay) {
        const { addOverlay, removeOverlay, getOverlayId, flashing } = get();

        const id = getOverlayId(overlay.from, overlay.to);
        if (flashing.has(id)) return;

        set((state) => {
            state.flashing.add(id);
        });

        addOverlay(overlay);
        setTimeout(() => {
            removeOverlay(id);

            set((state) => {
                state.flashing.delete(id);
            });
        }, 300);
    },
});
