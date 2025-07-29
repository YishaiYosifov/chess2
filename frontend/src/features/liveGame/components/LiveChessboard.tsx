"use client";

import { useCallback, useRef } from "react";

import { LogicalPoint } from "@/types/tempModels";
import { useGameEmitter } from "@/features/signalR/hooks/useSignalRHubs";
import LiveChessboardProfile, {
    ProfileSide as ChessProfileSide,
} from "./LiveChessboardProfile";
import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";
import ChessboardLayout from "@/features/chessboard/components/ChessboardLayout";
import {
    ChessboardState,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import MoveHistoryTable from "./MoveHistoryTable";
import GameControls from "./GameControls";
import GameChat from "./GameChat";
import GameOverPopup, { GameOverPopupRef } from "./GameOverPopup";
import LiveChessStoreContext from "../contexts/liveChessContext";
import { useLiveChessEvents } from "../hooks/useLiveChessEvents";
import { StoreApi } from "zustand";
import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import { GameState } from "@/lib/apiClient";
import {
    createStoreProps,
    ProcessedGameState,
} from "../lib/gameStateProcessor";
import useConst from "@/hooks/useConst";

const LiveChessboard = ({
    gameToken,
    gameState,
}: {
    gameToken: string;
    gameState: GameState;
}) => {
    const user = useSessionUser();
    const gameOverPopupRef = useRef<GameOverPopupRef>(null);

    const sendGameEvent = useGameEmitter(gameToken);
    const sendMove = useCallback(
        async (from: LogicalPoint, to: LogicalPoint) => {
            await sendGameEvent("MovePieceAsync", gameToken, from, to);
        },
        [sendGameEvent, gameToken],
    );

    const storeProps = useConst<ProcessedGameState>(() =>
        createStoreProps(gameToken, user?.userId ?? "", gameState),
    );

    const chessboardStore = useConst<StoreApi<ChessboardState>>(() =>
        createChessboardStore({
            ...storeProps.board,
            onPieceMovement: sendMove,
        }),
    );

    const liveChessStore = useConst<StoreApi<LiveChessStore>>(() =>
        createLiveChessStore(storeProps.live),
    );

    useLiveChessEvents(
        gameToken,
        liveChessStore,
        chessboardStore,
        gameOverPopupRef,
        user?.userId,
    );

    return (
        <LiveChessStoreContext.Provider value={liveChessStore}>
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <GameOverPopup ref={gameOverPopupRef} />
                <div
                    className="flex w-full flex-col items-center justify-center gap-5 p-5 lg:max-h-screen
                        lg:flex-row lg:items-start"
                >
                    <section className="flex h-max w-fit flex-col gap-3">
                        <LiveChessboardProfile
                            side={ChessProfileSide.Opponent}
                        />
                        <ChessboardLayout
                            breakpoints={[
                                {
                                    maxScreenSize: 767,
                                    paddingOffset: { width: 40, height: 258 },
                                },
                                {
                                    maxScreenSize: 1024,
                                    paddingOffset: { width: 200, height: 198 },
                                },
                            ]}
                            defaultOffset={{ width: 626, height: 164 }}
                            className="self-center"
                        />
                        <LiveChessboardProfile
                            side={ChessProfileSide.CurrentlyPlaying}
                        />
                    </section>

                    <aside
                        className="grid h-full w-full max-w-96 min-w-xs grid-rows-[minmax(100px,3fr)_70px_200px]
                            gap-3 overflow-auto lg:max-w-sm"
                    >
                        <MoveHistoryTable />
                        <GameControls />
                        <GameChat />
                    </aside>
                </div>
            </ChessboardStoreContext.Provider>
        </LiveChessStoreContext.Provider>
    );
};
export default LiveChessboard;
