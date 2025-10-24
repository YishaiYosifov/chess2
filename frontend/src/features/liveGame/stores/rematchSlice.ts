import { StateCreator } from "zustand";
import { LiveChessStore } from "./liveChessStore";

export interface RematchSlice {
    isRequestingRematch: boolean;
    isRematchRequested: boolean;

    setRematchRequested(isRequested: boolean): void;
    setRequestingRematch(isRequesting: boolean): void;
}

export const createRematchSlice: StateCreator<
    LiveChessStore,
    [["zustand/immer", never], never],
    [],
    RematchSlice
> = (set) => ({
    isRequestingRematch: false,
    isRematchRequested: false,

    setRematchRequested(isRequested) {
        set((state) => {
            state.isRematchRequested = isRequested;
        });
    },
    setRequestingRematch(isRequesting) {
        set((state) => {
            state.isRequestingRematch = isRequesting;
        });
    },
});
