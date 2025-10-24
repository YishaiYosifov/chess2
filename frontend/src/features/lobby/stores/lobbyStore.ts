import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { devtools } from "zustand/middleware";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";

import { PoolKeyStr } from "../lib/types";

interface LobbyStore {
    seeks: Set<PoolKeyStr>;
    requestedOpenSeek: boolean;

    clearSeeks(): void;
    addSeek(pool: PoolKeyStr): void;
    removeSeek(pool: PoolKeyStr): void;

    setRequestedOpenSeek(isRequesting: boolean): void;
}

enableMapSet();
const useLobbyStore = createWithEqualityFn<LobbyStore>()(
    devtools(
        immer((set) => ({
            seeks: new Set(),
            requestedOpenSeek: false,

            clearSeeks() {
                set((state) => {
                    state.seeks.clear();
                });
            },
            addSeek(pool) {
                set((state) => {
                    state.seeks.add(pool);
                });
            },
            removeSeek(pool) {
                set((state) => {
                    state.seeks.delete(pool);
                });
            },

            setRequestedOpenSeek(isRequesting) {
                set((state) => {
                    state.requestedOpenSeek = isRequesting;
                });
            },
        })),
    ),
    shallow,
);
export default useLobbyStore;
