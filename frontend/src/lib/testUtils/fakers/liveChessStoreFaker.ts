import createLiveChessStore, {
    LiveChessStore,
    RequiredLiveChessData,
} from "@/features/liveGame/stores/liveChessStore";
import { GameColor } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";
import { createPlayer } from "./playerFaker";
import { StoreApi } from "zustand";

export function createFakeLiveChessStore(
    override?: Partial<RequiredLiveChessData>,
): StoreApi<LiveChessStore> {
    return createLiveChessStore({
        gameToken: faker.string.alpha(16),
        moveHistory: [],

        sideToMove: faker.helpers.enumValue(GameColor),
        playerColor: faker.helpers.enumValue(GameColor),
        whitePlayer: createPlayer(GameColor.WHITE),
        blackPlayer: createPlayer(GameColor.BLACK),

        clocks: {
            whiteClock: faker.number.int({ min: 10000, max: 100000 }),
            blackClock: faker.number.int({ min: 10000, max: 100000 }),
            lastUpdated: Date.now().valueOf(),
        },

        ...override,
    });
}
