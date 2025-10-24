import { StoreApi } from "zustand";
import createLiveChessStore, { LiveChessStore } from "../liveChessStore";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";

describe("RematchSlice", () => {
    let store: StoreApi<LiveChessStore>;

    beforeEach(() => {
        const initialProps = createFakeLiveChessStoreProps();
        store = createLiveChessStore(initialProps);
    });

    it("should initially set isRequestingRematch to false", () => {
        expect(store.getState().isRequestingRematch).toBe(false);
    });

    it("should initially set isRematchRequested to false", () => {
        expect(store.getState().isRematchRequested).toBe(false);
    });

    describe("setRematchRequested", () => {
        it("should set isRematchRequested to true", () => {
            store.getState().setRematchRequested(true);
            expect(store.getState().isRematchRequested).toBe(true);
        });

        it("should set isRematchRequested to false", () => {
            const { setRematchRequested } = store.getState();

            setRematchRequested(true);
            expect(store.getState().isRematchRequested).toBe(true);

            setRematchRequested(false);
            expect(store.getState().isRematchRequested).toBe(false);
        });
    });

    describe("setRequestingRematch", () => {
        it("should set isRequestingRematch to true", () => {
            store.getState().setRequestingRematch(true);
            expect(store.getState().isRequestingRematch).toBe(true);
        });

        it("should set isRequestingRematch to false", () => {
            const { setRequestingRematch } = store.getState();

            setRequestingRematch(true);
            expect(store.getState().isRequestingRematch).toBe(true);

            setRequestingRematch(false);
            expect(store.getState().isRequestingRematch).toBe(false);
        });
    });
});
