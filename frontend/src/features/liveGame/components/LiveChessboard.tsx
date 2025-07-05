"use client";

import { Point } from "@/types/tempModels";
import {
    useGameEmitter,
    useGameEvent,
} from "@/features/signalR/hooks/useSignalRHubs";
import { useEffect, useMemo, useCallback, useRef } from "react";
import {
    decodeMoves,
    decodeMovesIntoMap,
    decodeSingleMove,
} from "@/lib/chessDecoders/moveDecoder";
import { GameColor, GameState, getLiveGame } from "@/lib/apiClient";
import { decodeFen } from "@/lib/chessDecoders/fenDecoder";
import LiveChessboardProfile, {
    ProfileSide as ChessProfileSide,
} from "./LiveChessboardProfile";
import useLiveChessboardStore from "@/features/liveGame/stores/liveChessboardStore";
import ChessboardLayout from "@/features/chessboard/components/ChessboardLayout";
import { createChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { ChessStoreContext } from "@/features/chessboard/contexts/chessStoreContext";
import MoveHistoryTable from "./MoveHistoryTable";
import GameControls from "./GameControls";
import GameChat from "./GameChat";
import GameOverPopup, { GameOverPopupRef } from "./GameOverPopup";

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

    async function refetchGame() {
        const { error, data } = await getLiveGame({ path: { gameToken } });
        if (error || !data) {
            console.error(error);
            return;
        }

        const pieces = decodeFen(data.fen);
        const legalMoves = decodeMovesIntoMap(data.legalMoves);
        chessboardStore
            .getState()
            .resetState(pieces, legalMoves, data.sideToMove);

        const moveHistory = decodeMoves(data.moveHistory);
        const { setMoveHistory } = useLiveChessboardStore.getState();
        setMoveHistory(moveHistory);
    }

    const sendGameEvent = useGameEmitter(gameToken);
    useGameEvent(
        gameToken,
        "MoveMadeAsync",
        async (move: string, sideToMove: GameColor, moveNumber: number) => {
            // we missed a move... we need to refetch the state
            const { moveHistory, addMoveToHistory } =
                useLiveChessboardStore.getState();
            if (moveNumber != moveHistory.length + 1) {
                await refetchGame();
                return;
            }

            const decodedMove = decodeSingleMove(move);
            addMoveToHistory(decodedMove);

            chessboardStore
                .getState()
                .playTurn(sideToMove == playerColor ? decodedMove : undefined);
        },
    );

    useGameEvent(
        gameToken,
        "LegalMovesChangedAsync",
        async (legalMoves: string[]) => {
            const decodedLegalMoves = decodeMovesIntoMap(legalMoves);
            chessboardStore.getState().setLegalMoves(decodedLegalMoves);
        },
    );

    useGameEvent(
        gameToken,
        "GameEndedAsync",
        async (result, resultDescription, newWhiteRating, newBlackRating) => {
            useLiveChessboardStore
                .getState()
                .endGame(
                    result,
                    resultDescription,
                    newWhiteRating,
                    newBlackRating,
                );
            chessboardStore.getState().disableMovement();
            gameOverPopupRef.current?.open();
        },
    );

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

    useEffect(() => {
        const decodedMoveHistory = decodeMoves(gameState.moveHistory);
        const { setMoveHistory, setPlayers, setGameToken } =
            useLiveChessboardStore.getState();

        setMoveHistory(decodedMoveHistory);
        setPlayers(gameState.whitePlayer, gameState.blackPlayer, playerColor);
        setGameToken(gameToken);
    }, [gameState, playerColor, gameToken]);

    return (
        <ChessStoreContext.Provider value={chessboardStore}>
            <GameOverPopup ref={gameOverPopupRef} />
            <div
                className="flex w-full flex-col items-center justify-center gap-5 p-5 lg:flex-row
                    lg:items-start"
            >
                <section className="flex h-max w-fit flex-col gap-3">
                    <LiveChessboardProfile side={ChessProfileSide.Opponent} />
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
                    className="grid h-full w-full min-w-xs grid-rows-[minmax(100px,3fr)_70px_200px] gap-3
                        overflow-auto lg:max-w-xs"
                >
                    <MoveHistoryTable />
                    <GameControls />
                    <GameChat />
                </aside>
            </div>
        </ChessStoreContext.Provider>
    );
};
export default LiveChessboard;
