import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { devtools } from "zustand/middleware";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";

import { OngoingGame, PoolKeyStr } from "../lib/types";

interface LobbyStore {
    seeks: Set<PoolKeyStr>;
    requestedOpenSeek: boolean;
    ongoingGames: Map<string, OngoingGame>;

    clearSeeks(): void;
    addSeek(pool: PoolKeyStr): void;
    removeSeek(pool: PoolKeyStr): void;

    setRequestedOpenSeek(isRequesting: boolean): void;

    addOngoingGames(games: OngoingGame[]): void;
    removeOngoingGame(gameToken: string): void;
}

enableMapSet();
const useLobbyStore = createWithEqualityFn<LobbyStore>()(
    devtools(
        immer((set) => ({
            seeks: new Set(),
            requestedOpenSeek: false,
            ongoingGames: new Map(),

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

            addOngoingGames(games) {
                set((state) => {
                    for (const game of games) {
                        state.ongoingGames.set(game.gameToken, game);
                    }
                });
            },
            removeOngoingGame(gameToken) {
                set((state) => {
                    state.ongoingGames.delete(gameToken);
                });
            },
        })),
    ),
    shallow,
);
export default useLobbyStore;
