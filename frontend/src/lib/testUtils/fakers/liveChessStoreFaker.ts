import createLiveChessStore, {
    LiveChessStore,
    RequiredLiveChessData,
} from "@/features/liveGame/stores/liveChessboardStore";
import { GameColor } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";
import { createPlayer } from "./playerFaker";
import { StoreApi } from "zustand";

export function createFakeLiveChessStore(
    override?: Partial<RequiredLiveChessData>,
): StoreApi<LiveChessStore> {
    return createLiveChessStore({
        gameToken: faker.string.alpha(16),
        playerColor: faker.helpers.enumValue(GameColor),
        whitePlayer: createPlayer(GameColor.WHITE),
        blackPlayer: createPlayer(GameColor.BLACK),
        moveHistory: [],
        ...override,
    });
}
