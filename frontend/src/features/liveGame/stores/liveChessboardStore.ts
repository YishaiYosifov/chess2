import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";

import { GameColor, GamePlayer } from "@/lib/apiClient";
import { GameResult, Move } from "@/types/tempModels";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";

export interface GameResultData {
    result: GameResult;
    resultDescription: string;
    whiteRatingDelta?: number;
    blackRatingDelta?: number;
}

export interface RequiredLiveChessData {
    gameToken: string;
    moveHistory: Move[];
    playerColor: GameColor;
    whitePlayer: GamePlayer;
    blackPlayer: GamePlayer;
}

export interface LiveChessStore extends RequiredLiveChessData {
    resultData?: GameResultData;

    addMoveToHistory(move: Move): void;
    setMoveHistory(moveHistory: Move[]): void;
    endGame(
        result: GameResult,
        resultDescription: string,
        newWhiteRating?: number,
        newBlackRating?: number,
    ): void;
}

enableMapSet();
export default function createLiveChessStore(initState: RequiredLiveChessData) {
    return createWithEqualityFn<LiveChessStore>()(
        immer((set, get) => ({
            ...initState,

            addMoveToHistory: (move: Move) =>
                set((state) => {
                    state.moveHistory.push(move);
                }),
            setMoveHistory: (moveHistory: Move[]) =>
                set((state) => {
                    state.moveHistory = moveHistory;
                }),

            endGame(
                result: GameResult,
                resultDescription: string,
                newWhiteRating?: number,
                newBlackRating?: number,
            ) {
                const { whitePlayer, blackPlayer } = get();

                const whiteRatingDelta =
                    newWhiteRating && whitePlayer?.rating
                        ? newWhiteRating - whitePlayer.rating
                        : undefined;

                const blackRatingDelta =
                    newBlackRating && blackPlayer?.rating
                        ? newBlackRating - blackPlayer.rating
                        : undefined;

                set((state) => {
                    if (state.whitePlayer)
                        state.whitePlayer.rating = newWhiteRating;
                    if (state.blackPlayer)
                        state.blackPlayer.rating = newBlackRating;
                    state.resultData = {
                        result: result,
                        resultDescription: resultDescription,
                        whiteRatingDelta,
                        blackRatingDelta,
                    };
                });
            },
        })),
        shallow,
    );
}
