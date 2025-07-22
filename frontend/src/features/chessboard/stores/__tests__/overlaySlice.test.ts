import { StoreApi } from "zustand";
import { ChessboardState, createChessboardStore } from "../chessboardStore";
import { OverlayItem } from "../overlaySlice";

describe("OverlaySlice", () => {
    let store: StoreApi<ChessboardState>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    describe("setCurrentlyDrawing", () => {
        it("should set the currentlyDrawing overlay", () => {
            const slice = store.getState();
            const from = { x: 0, y: 0 };
            const to = { x: 1, y: 1 };

            slice.setCurrentlyDrawing(from, to);

            expect(store.getState().currentlyDrawing).toEqual<OverlayItem>({
                from,
                to,
            });
        });
    });

    describe("commitCurrentlyDrawing", () => {
        it("should do nothing if currentlyDrawing is null", () => {
            const slice = store.getState();

            slice.commitCurrentlyDrawing();

            expect(store.getState().overlays.size).toBe(0);
            expect(store.getState().currentlyDrawing).toBeNull();
        });

        it("should add a new overlay when the id is not present", () => {
            const slice = store.getState();
            const from = { x: 2, y: 2 };
            const to = { x: 3, y: 3 };
            const id = "2,2-3,3";

            slice.setCurrentlyDrawing(from, to);
            slice.commitCurrentlyDrawing();

            expect(store.getState().overlays.has(id)).toBe(true);
            expect(store.getState().currentlyDrawing).toBeNull();
        });

        it("should remove an existing overlay with the same id (toggle off)", () => {
            const slice = store.getState();
            const from = { x: 4, y: 4 };
            const to = { x: 5, y: 5 };
            const id = "4,4-5,5";

            slice.setCurrentlyDrawing(from, to);
            slice.commitCurrentlyDrawing();
            expect(store.getState().overlays.has(id)).toBe(true);

            slice.setCurrentlyDrawing(from, to);
            slice.commitCurrentlyDrawing();

            expect(store.getState().overlays.has(id)).toBe(false);
            expect(store.getState().currentlyDrawing).toBeNull();
        });
    });

    describe("clearOverlays", () => {
        it("should clear overlays and currentlyDrawing", () => {
            const slice = store.getState();
            const from = { x: 6, y: 6 };
            const to = { x: 7, y: 7 };

            slice.setCurrentlyDrawing(from, to);
            slice.commitCurrentlyDrawing();

            expect(store.getState().overlays.size).toBeGreaterThan(0);

            slice.clearOverlays();

            expect(store.getState().overlays.size).toBe(0);
            expect(store.getState().currentlyDrawing).toBeNull();
        });
    });
});
