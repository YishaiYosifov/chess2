import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";

import {
    createHistorySlice,
    HistorySlice,
    HistorySliceProps,
} from "./historySlice";

import {
    createGamePlaySlice,
    GamePlaySlice,
    GamePlaySliceProps,
} from "./gamePlaySlice";

import {
    createGameStateSlice,
    GameStateSlice,
    GameStateSliceProps,
} from "./gameStateSlice";

import { createRematchSlice, RematchSlice } from "./rematchSlice";

export type LiveChessStoreProps = HistorySliceProps &
    GamePlaySliceProps &
    GameStateSliceProps;

export type LiveChessStore = HistorySlice &
    GamePlaySlice &
    GameStateSlice &
    RematchSlice;

enableMapSet();
export default function createLiveChessStore(initState: LiveChessStoreProps) {
    return createWithEqualityFn<LiveChessStore>()(
        immer((...a) => ({
            ...createHistorySlice(initState)(...a),
            ...createGamePlaySlice(initState)(...a),
            ...createGameStateSlice(initState)(...a),
            ...createRematchSlice(...a),
        })),
        shallow,
    );
}
