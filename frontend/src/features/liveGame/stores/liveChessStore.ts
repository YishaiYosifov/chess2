import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";

import {
    Clocks,
    GameColor,
    GamePlayer,
    GameResultData,
    MoveSnapshot,
} from "@/lib/apiClient";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";

export interface LiveChessStoreProps {
    gameToken: string;
    moveHistory: MoveSnapshot[];

    sideToMove: GameColor;
    playerColor: GameColor;
    whitePlayer: GamePlayer;
    blackPlayer: GamePlayer;

    clocks: Clocks;
    resultData: GameResultData | null;
}

export interface LiveChessStore extends LiveChessStoreProps {
    receiveMove(
        move: MoveSnapshot,
        clocks: Clocks,
        sideToMove: GameColor,
    ): void;
    setMoveHistory(moveHistory: MoveSnapshot[]): void;
    endGame(resultData: GameResultData): void;
}

enableMapSet();
export default function createLiveChessStore(initState: LiveChessStoreProps) {
    return createWithEqualityFn<LiveChessStore>()(
        immer((set) => ({
            ...initState,

            receiveMove: (
                move: MoveSnapshot,
                clocks: Clocks,
                sideToMove: GameColor,
            ) =>
                set((state) => {
                    state.moveHistory.push(move);
                    state.clocks = clocks;
                    state.sideToMove = sideToMove;
                }),

            setMoveHistory: (moveHistory: MoveSnapshot[]) =>
                set((state) => {
                    state.moveHistory = moveHistory;
                }),

            endGame(resultData: GameResultData) {
                set((state) => {
                    if (
                        state.whitePlayer.rating &&
                        resultData.whiteRatingChange
                    )
                        state.whitePlayer.rating +=
                            resultData.whiteRatingChange;
                    if (
                        state.blackPlayer.rating &&
                        resultData.blackRatingChange
                    )
                        state.blackPlayer.rating +=
                            resultData.blackRatingChange;

                    state.resultData = resultData;
                });
            },
        })),
        shallow,
    );
}
