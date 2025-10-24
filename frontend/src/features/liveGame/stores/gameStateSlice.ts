import { StateCreator } from "zustand";

import { createMoveOptions } from "@/features/chessboard/lib/moveOptions";
import {
    DrawState,
    GamePlayer,
    GameResultData,
    PoolKey,
} from "@/lib/apiClient";
import { LiveChessStore, LiveChessStoreProps } from "./liveChessStore";

export interface GameStateSliceProps {
    gameToken: string;

    whitePlayer: GamePlayer;
    blackPlayer: GamePlayer;

    sourceRevision: number;
    pool: PoolKey;

    resultData: GameResultData | null;
    drawState: DrawState;
}

export interface GameStateSlice extends GameStateSliceProps {
    decrementDrawCooldown(): void;
    drawStateChange(drawState: DrawState): void;
    endGame(resultData: GameResultData): void;

    resetState(initState: LiveChessStoreProps): void;
}

export function createGameStateSlice(
    initState: GameStateSliceProps,
): StateCreator<
    LiveChessStore,
    [["zustand/immer", never], never],
    [],
    GameStateSlice
> {
    return (set, _, store) => ({
        ...initState,

        decrementDrawCooldown() {
            set((state) => {
                state.drawState.whiteCooldown = Math.max(
                    0,
                    state.drawState.whiteCooldown - 1,
                );
                state.drawState.blackCooldown = Math.max(
                    0,
                    state.drawState.blackCooldown - 1,
                );
            });
        },

        drawStateChange(drawState) {
            set((state) => {
                state.drawState = drawState;
            });
        },

        endGame(resultData) {
            set((state) => {
                if (state.whitePlayer.rating && resultData.whiteRatingChange)
                    state.whitePlayer.rating += resultData.whiteRatingChange;
                if (state.blackPlayer.rating && resultData.blackRatingChange)
                    state.blackPlayer.rating += resultData.blackRatingChange;

                state.latestMoveOptions = createMoveOptions();
                state.resultData = resultData;
            });
        },

        resetState(initState) {
            set(() => ({ ...store.getInitialState(), ...initState }));
        },
    });
}
