import { PoolKeyStr } from "../lib/types";
import { enableMapSet } from "immer";
import { createWithEqualityFn } from "zustand/traditional";
import { shallow } from "zustand/shallow";
import { immer } from "zustand/middleware/immer";
import { devtools } from "zustand/middleware";

interface LobbyStore {
    seeks: Set<PoolKeyStr>;

    clearSeeks(): void;
    addSeek(pool: PoolKeyStr): void;
    removeSeek(pool: PoolKeyStr): void;
}

enableMapSet();
const useLobbyStore = createWithEqualityFn<LobbyStore>()(
    devtools(
        immer((set) => ({
            seeks: new Set(),

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
        })),
    ),
    shallow,
);
export default useLobbyStore;
