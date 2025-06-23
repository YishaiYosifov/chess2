"use client";

import { Point } from "@/types/tempModels";
import { useGameEmitter, useGameEvent } from "@/hooks/signalR/useSignalRHubs";
import { useEffect, useMemo, useCallback } from "react";
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
import useLiveChessboardStore from "@/stores/liveChessboardStore";
import ChessboardLayout from "../game/ChessboardLayout";
import { createChessboardStore } from "@/stores/chessboardStore";
import { ChessStoreContext } from "@/contexts/chessStoreContext";

const LiveChessboard = ({
    gameToken,
    gameState,
    userId,
}: {
    gameToken: string;
    gameState: GameState;
    userId: string;
}) => {
    const playingAs =
        userId == gameState.playerWhite.userId
            ? gameState.playerWhite
            : gameState.playerBlack;

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
        async (
            move: string,
            legalMoves: string[],
            sideToMove: GameColor,
            moveNumber: number,
        ) => {
            // we missed a move... we need to refetch the state
            const { moveHistory, setMoveHistory } =
                useLiveChessboardStore.getState();
            if (moveNumber != moveHistory.length + 1) {
                await refetchGame();
                return;
            }

            const decodedMove = decodeSingleMove(move);
            const decodedLegalMoves = decodeMovesIntoMap(legalMoves);
            setMoveHistory([...moveHistory, decodedMove]);

            chessboardStore
                .getState()
                .playTurn(
                    decodedLegalMoves,
                    sideToMove,
                    sideToMove == playingAs.color ? decodedMove : undefined,
                );
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

            viewingFrom: playingAs.color,
            playingAs: playingAs.color,

            sideToMove: gameState.sideToMove,
            onPieceMovement: sendMove,
        });
    }, [gameState, sendMove, playingAs.color]);

    useEffect(() => {
        const decodedMoveHistory = decodeMoves(gameState.moveHistory);
        const { setMoveHistory, setPlayers } =
            useLiveChessboardStore.getState();

        setMoveHistory(decodedMoveHistory);
        setPlayers(gameState.playerWhite, gameState.playerBlack);
    }, [gameState, playingAs.color]);

    return (
        <ChessStoreContext.Provider value={chessboardStore}>
            <div className="flex flex-col gap-3">
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
                    defaultOffset={{ width: 626, height: 148 }}
                />
                <LiveChessboardProfile
                    side={ChessProfileSide.CurrentlyPlaying}
                />
            </div>
        </ChessStoreContext.Provider>
    );
};
export default LiveChessboard;
