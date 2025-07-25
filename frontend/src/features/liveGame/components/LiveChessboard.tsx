"use client";

import { LogicalPoint } from "@/types/tempModels";
import { useGameEmitter } from "@/features/signalR/hooks/useSignalRHubs";
import { useCallback, useRef } from "react";
import LiveChessboardProfile, {
    ProfileSide as ChessProfileSide,
} from "./LiveChessboardProfile";
import createLiveChessStore, {
    LiveChessStore,
    LiveChessStoreProps,
} from "@/features/liveGame/stores/liveChessStore";
import ChessboardLayout from "@/features/chessboard/components/ChessboardLayout";
import {
    ChessboardProps,
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

const LiveChessboard = ({
    gameToken,
    userId,
    liveProps,
    boardProps,
}: {
    gameToken: string;
    userId: string;
    liveProps: LiveChessStoreProps;
    boardProps: ChessboardProps;
}) => {
    const gameOverPopupRef = useRef<GameOverPopupRef>(null);

    const sendGameEvent = useGameEmitter(gameToken);
    const sendMove = useCallback(
        async (from: LogicalPoint, to: LogicalPoint) => {
            await sendGameEvent("MovePieceAsync", gameToken, from, to);
        },
        [sendGameEvent, gameToken],
    );

    const chessboardStoreRef = useRef<StoreApi<ChessboardState> | null>(null);
    if (!chessboardStoreRef.current)
        chessboardStoreRef.current = createChessboardStore({
            ...boardProps,
            onPieceMovement: sendMove,
        });

    const liveChessStoreRef = useRef<StoreApi<LiveChessStore> | null>(null);
    if (!liveChessStoreRef.current)
        liveChessStoreRef.current = createLiveChessStore(liveProps);

    useLiveChessEvents(
        gameToken,
        userId,
        liveChessStoreRef.current,
        chessboardStoreRef.current,
        gameOverPopupRef,
    );

    return (
        <LiveChessStoreContext.Provider value={liveChessStoreRef.current}>
            <ChessboardStoreContext.Provider value={chessboardStoreRef.current}>
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
