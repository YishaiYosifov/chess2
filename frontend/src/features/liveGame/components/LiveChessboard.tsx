"use client";

import { Point } from "@/types/tempModels";
import { useGameEmitter } from "@/features/signalR/hooks/useSignalRHubs";
import { useMemo, useCallback, useRef } from "react";
import { decodeMovesIntoMap } from "@/lib/chessDecoders/moveDecoder";
import { GameState } from "@/lib/apiClient";
import { decodeFen } from "@/lib/chessDecoders/fenDecoder";
import LiveChessboardProfile, {
    ProfileSide as ChessProfileSide,
} from "./LiveChessboardProfile";
import createLiveChessStore from "@/features/liveGame/stores/liveChessStore";
import ChessboardLayout from "@/features/chessboard/components/ChessboardLayout";
import { createChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import MoveHistoryTable from "./MoveHistoryTable";
import GameControls from "./GameControls";
import GameChat from "./GameChat";
import GameOverPopup, { GameOverPopupRef } from "./GameOverPopup";
import LiveChessStoreContext from "../contexts/liveChessContext";
import { useLiveChessEvents } from "../hooks/useLiveChessEvents";

const LiveChessboard = ({
    gameToken,
    gameState,
    userId,
}: {
    gameToken: string;
    gameState: GameState;
    userId: string;
}) => {
    const playerColor =
        userId == gameState.whitePlayer.userId
            ? gameState.whitePlayer.color
            : gameState.blackPlayer.color;

    const gameOverPopupRef = useRef<GameOverPopupRef>(null);

    const sendGameEvent = useGameEmitter(gameToken);
    const sendMove = useCallback(
        async (from: Point, to: Point) => {
            await sendGameEvent("MovePieceAsync", gameToken, from, to);
        },
        [sendGameEvent, gameToken],
    );

    const chessboardStore = useMemo(() => {
        const decodedLegalMoves = decodeMovesIntoMap(gameState.legalMoves);
        const decodedFen = decodeFen(gameState.fen);

        return createChessboardStore({
            pieces: decodedFen,
            legalMoves: decodedLegalMoves,

            viewingFrom: playerColor,
            onPieceMovement: sendMove,
        });
    }, [gameState, sendMove, playerColor]);
    const liveChessStore = useMemo(() => {
        return createLiveChessStore({
            gameToken,

            whitePlayer: gameState.whitePlayer,
            blackPlayer: gameState.blackPlayer,
            playerColor,
            sideToMove: gameState.sideToMove,

            moveHistory: gameState.moveHistory,
            clocks: gameState.clocks,
            resultData: gameState.resultData ?? null,
        });
    }, [gameToken, gameState, playerColor]);

    useLiveChessEvents(
        gameToken,
        playerColor,
        liveChessStore,
        chessboardStore,
        gameOverPopupRef,
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
