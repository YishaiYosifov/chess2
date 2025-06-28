import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";

import { GameColor, GamePlayer } from "@/lib/apiClient";
import { GameResult, Move } from "@/types/tempModels";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";
import { devtools } from "zustand/middleware";

interface GameResultData {
    gameResult: GameResult;
    whiteRatingDelta?: number;
    blackRatingDelta?: number;
}

export interface LiveChessboardStore {
    gameToken: string;
    moveHistory: Move[];
    whitePlayer?: GamePlayer;
    blackPlayer?: GamePlayer;
    result?: GameResultData;

    setGameToken(gameToken: string): void;
    addMoveToHistory(move: Move): void;
    setMoveHistory(moveHistory: Move[]): void;
    setPlayers(whitePlayer: GamePlayer, blackPlayer: GamePlayer): void;
    endGame(
        gameResult: GameResult,
        newWhiteRating?: number,
        newBlackRating?: number,
    ): void;
}

enableMapSet();
const useLiveChessboardStore = createWithEqualityFn<LiveChessboardStore>()(
    devtools(
        immer((set, get) => ({
            gameToken: "",
            moveHistory: [],
            viewingFrom: GameColor.WHITE,
            players: { colorToPlayer: new Map<GameColor, GamePlayer>() },
            result: {
                gameResult: GameResult.WHITE_WIN,
                whiteRatingDelta: 10,
                blackRatingDelta: -10,
            },

            setGameToken: (gameToken: string) =>
                set((state) => {
                    state.gameToken = gameToken;
                }),
            addMoveToHistory: (move: Move) =>
                set((state) => {
                    state.moveHistory.push(move);
                }),
            setMoveHistory: (moveHistory: Move[]) =>
                set((state) => {
                    state.moveHistory = moveHistory;
                }),
            setPlayers: (whitePlayer: GamePlayer, blackPlayer: GamePlayer) =>
                set((state) => {
                    state.whitePlayer = whitePlayer;
                    state.blackPlayer = blackPlayer;
                }),

            endGame(
                gameResult: GameResult,
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
                    state.result = {
                        gameResult,
                        whiteRatingDelta,
                        blackRatingDelta,
                    };
                });
            },
        })),
        shallow,
    ),
);
export default useLiveChessboardStore;
