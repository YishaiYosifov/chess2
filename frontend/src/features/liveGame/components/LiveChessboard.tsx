"use client";

import { useCallback, useEffect } from "react";
import { StoreApi } from "zustand";

import LiveChessboardProfile, {
    ProfileSide as ChessProfileSide,
} from "./LiveChessboardProfile";
import createLiveChessStore, { LiveChessStore } from "../stores/liveChessStore";
import ChessboardLayout from "@/features/chessboard/components/ChessboardLayout";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import MoveHistoryTable from "./MoveHistoryTable";
import GameControlsCard from "./GameControls/GameControlsCard";
import GameChat from "./GameChat";
import GameOverPopup from "./GameOverPopup";
import LiveChessStoreContext from "../contexts/liveChessContext";
import useLiveChessEvents from "../hooks/useLiveChessEvents";
import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import { GameState, Preferences } from "@/lib/apiClient";
import useInvalidateOnNavigate from "@/hooks/useInvalidateOnNavigate";
import {
    createStoreProps,
    ProcessedGameState,
} from "../lib/gameStateProcessor";
import useConst from "@/hooks/useConst";
import { Move } from "@/features/chessboard/lib/types";
import ChessboardWithSidebar from "@/features/chessboard/components/ChessboardWithSidebar";
import { useGameEmitter } from "../hooks/useGameHub";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";

const LiveChessboard = ({
    gameToken,
    gameState,
    preferences,
}: {
    gameToken: string;
    gameState: GameState;
    preferences: Preferences;
}) => {
    const user = useSessionUser();

    const storeProps = useConst<ProcessedGameState>(() =>
        createStoreProps(gameToken, user?.userId ?? "", gameState),
    );

    const liveChessStore = useConst<StoreApi<LiveChessStore>>(() =>
        createLiveChessStore(storeProps.live),
    );

    useEffect(() => {
        if (gameState.moveHistory.length === 0 && !gameState.resultData) {
            AudioPlayer.playAudio(AudioType.GAME_START);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps -- only play sound once
    }, []);

    const sendGameEvent = useGameEmitter(gameToken);
    const sendMove = useCallback(
        async (move: Move) => {
            liveChessStore.getState().markPendingMoveAck();
            await sendGameEvent("MovePieceAsync", gameToken, move.moveKey);
        },
        [sendGameEvent, gameToken, liveChessStore],
    );

    const chessboardStore = useConst<StoreApi<ChessboardStore>>(() =>
        createChessboardStore({
            ...storeProps.board,
            onPieceMovement: sendMove,
        }),
    );

    useLiveChessEvents(liveChessStore, chessboardStore);
    useInvalidateOnNavigate();

    return (
        <LiveChessStoreContext.Provider value={liveChessStore}>
            <ChessboardStoreContext.Provider value={chessboardStore}>
                <GameOverPopup />
                <ChessboardWithSidebar
                    chessboard={
                        <>
                            <LiveChessboardProfile
                                side={ChessProfileSide.Opponent}
                            />
                            <ChessboardLayout
                                breakpoints={[
                                    {
                                        maxScreenSize: 767,
                                        paddingOffset: {
                                            width: 40,
                                            height: 258,
                                        },
                                    },
                                    {
                                        maxScreenSize: 1024,
                                        paddingOffset: {
                                            width: 200,
                                            height: 198,
                                        },
                                    },
                                ]}
                                defaultOffset={{ width: 626, height: 164 }}
                                className="mx-auto"
                            />
                            <LiveChessboardProfile
                                side={ChessProfileSide.CurrentlyPlaying}
                            />
                        </>
                    }
                    aside={
                        <aside
                            className="grid h-full w-full min-w-xs grid-rows-[minmax(100px,3fr)_100px_200px] gap-3
                                overflow-auto lg:max-w-sm"
                        >
                            <MoveHistoryTable />
                            <GameControlsCard />
                            <GameChat initialShowChat={preferences.showChat} />
                        </aside>
                    }
                />
            </ChessboardStoreContext.Provider>
        </LiveChessStoreContext.Provider>
    );
};
export default LiveChessboard;
