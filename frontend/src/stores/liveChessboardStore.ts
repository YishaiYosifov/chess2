import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";

import { GameColor, GamePlayer } from "@/lib/apiClient";
import { Move } from "@/types/tempModels";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";
import { devtools } from "zustand/middleware";

interface PlayerRoster {
    whitePlayer?: GamePlayer;
    blackPlayer?: GamePlayer;
    colorToPlayer: Map<GameColor, GamePlayer>;
}

export interface LiveChessboardStore {
    moveHistory: Move[];
    players: PlayerRoster;

    setMoveHistory: (moveHistory: Move[]) => void;
    setPlayers: (whitePlayer: GamePlayer, blackPlayer: GamePlayer) => void;
}

enableMapSet();
const useLiveChessboardStore = createWithEqualityFn<LiveChessboardStore>()(
    devtools(
        immer((set, get) => ({
            moveHistory: [],
            viewingFrom: GameColor.WHITE,
            players: { colorToPlayer: new Map<GameColor, GamePlayer>() },

            setMoveHistory: (moveHistory: Move[]) =>
                set((state) => {
                    state.moveHistory = moveHistory;
                }),

            setPlayers: (whitePlayer: GamePlayer, blackPlayer: GamePlayer) =>
                set((state) => {
                    state.players.whitePlayer = whitePlayer;
                    state.players.blackPlayer = blackPlayer;
                    state.players.colorToPlayer = new Map<
                        GameColor,
                        GamePlayer
                    >([
                        [GameColor.WHITE, whitePlayer],
                        [GameColor.BLACK, blackPlayer],
                    ]);
                }),
        })),
        shallow,
    ),
);
export default useLiveChessboardStore;
