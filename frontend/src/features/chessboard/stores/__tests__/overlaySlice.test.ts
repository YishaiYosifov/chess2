import { StoreApi } from "zustand";
import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { OverlayItem } from "../overlaySlice";
import { viewPoint } from "@/features/point/pointUtils";

describe("OverlaySlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    describe("setCurrentlyDrawing", () => {
        it("should set the currentlyDrawing overlay", () => {
            const slice = store.getState();
            const from = viewPoint({ x: 0, y: 0 });
            const to = viewPoint({ x: 1, y: 1 });

            slice.setCurrentlyDrawing(from, to);

            expect(store.getState().currentlyDrawing).toEqual<OverlayItem>({
                from,
                to,
            });
        });
    });

    describe("getOverlayId", () => {
        it("should return a consistent string id from two points", () => {
            const slice = store.getState();
            const from = viewPoint({ x: 2, y: 2 });
            const to = viewPoint({ x: 3, y: 3 });

            const id = slice.getOverlayId(from, to);
            expect(id).toBe("2,2-3,3");
        });
    });

    describe("addOverlay", () => {
        it("should add an overlay to the map", () => {
            const slice = store.getState();
            const from = viewPoint({ x: 4, y: 4 });
            const to = viewPoint({ x: 5, y: 5 });
            const overlay: OverlayItem = { from, to };

            const id = slice.getOverlayId(from, to);
            slice.addOverlay(overlay);

            expect(store.getState().overlays.has(id)).toBe(true);
            expect(store.getState().overlays.get(id)).toEqual(overlay);
        });
    });

    describe("removeOverlay", () => {
        it("should remove an overlay by id", () => {
            const slice = store.getState();
            const from = viewPoint({ x: 6, y: 6 });
            const to = viewPoint({ x: 7, y: 7 });
            const overlay: OverlayItem = { from, to };
            const id = slice.getOverlayId(from, to);

            slice.addOverlay(overlay);
            expect(store.getState().overlays.has(id)).toBe(true);

            slice.removeOverlay(id);
            expect(store.getState().overlays.has(id)).toBe(false);
        });
    });

    describe("toggleOverlay", () => {
        it("should add overlay if it does not exist", () => {
            const slice = store.getState();
            const from = viewPoint({ x: 8, y: 8 });
            const to = viewPoint({ x: 9, y: 9 });
            const overlay: OverlayItem = { from, to };
            const id = slice.getOverlayId(from, to);

            slice.toggleOverlay(overlay);
            expect(store.getState().overlays.has(id)).toBe(true);
        });

        it("should remove overlay if it already exists", () => {
            const slice = store.getState();
            const from = viewPoint({ x: 10, y: 10 });
            const to = viewPoint({ x: 11, y: 11 });
            const overlay: OverlayItem = { from, to };
            const id = slice.getOverlayId(from, to);

            slice.addOverlay(overlay);
            expect(store.getState().overlays.has(id)).toBe(true);

            slice.toggleOverlay(overlay);
            expect(store.getState().overlays.has(id)).toBe(false);
        });
    });

    describe("commitCurrentlyDrawing", () => {
        it("should do nothing if currentlyDrawing is null", () => {
            const slice = store.getState();

            slice.commitCurrentlyDrawing();

            expect(store.getState().overlays.size).toBe(0);
            expect(store.getState().currentlyDrawing).toBeNull();
        });

        it("should toggle the currentlyDrawing overlay and clear it", () => {
            const slice = store.getState();
            const from = viewPoint({ x: 12, y: 12 });
            const to = viewPoint({ x: 13, y: 13 });
            const id = slice.getOverlayId(from, to);

            slice.setCurrentlyDrawing(from, to);
            slice.commitCurrentlyDrawing();

            expect(store.getState().overlays.has(id)).toBe(true);
            expect(store.getState().currentlyDrawing).toBeNull();

            // commit again with same points = toggle it off
            slice.setCurrentlyDrawing(from, to);
            slice.commitCurrentlyDrawing();

            expect(store.getState().overlays.has(id)).toBe(false);
            expect(store.getState().currentlyDrawing).toBeNull();
        });
    });

    describe("clearOverlays", () => {
        it("should clear overlays and currentlyDrawing", () => {
            const slice = store.getState();
            const from = viewPoint({ x: 14, y: 14 });
            const to = viewPoint({ x: 15, y: 15 });

            slice.setCurrentlyDrawing(from, to);
            slice.commitCurrentlyDrawing();

            expect(store.getState().overlays.size).toBeGreaterThan(0);

            slice.clearOverlays();

            expect(store.getState().overlays.size).toBe(0);
            expect(store.getState().currentlyDrawing).toBeNull();
        });
    });
});
