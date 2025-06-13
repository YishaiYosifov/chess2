"use client";

import { Move, Point } from "@/types/tempModels";
import Chessboard, { ChessboardProps, ChessboardRef } from "../game/Chessboard";
import { useGameEmitter, useGameEvent } from "@/hooks/signalR/useSignalRHubs";
import { useRef, useState } from "react";
import {
    decodeMoves,
    decodeMovesIntoMap,
    decodeSingleMove,
} from "@/lib/chessDecoders/moveDecoder";
import { GameColor, getLiveGame } from "@/lib/apiClient";
import { decodeFen } from "@/lib/chessDecoders/fenDecoder";

const LiveChessboard = ({
    gameToken,
    initialMoveHistory,
    ...chessboardProps
}: { gameToken: string; initialMoveHistory: Move[] } & ChessboardProps) => {
    const chessboardRef = useRef<ChessboardRef>(null);
    const [moveHistory, setMoveHistory] = useState<Move[]>(initialMoveHistory);

    async function refetchGame() {
        const { error, data } = await getLiveGame({ path: { gameToken } });
        if (error || !data) {
            console.error(error);
            return;
        }

        const pieces = decodeFen(data.fen);
        const legalMoves = decodeMovesIntoMap(data.legalMoves);
        chessboardRef.current?.resetState(
            pieces,
            legalMoves,
            data.currentPlayerColor,
        );

        const moveHistory = decodeMoves(data.moveHistory);
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
            // TODO:
            // we missed a move... we need to refetch the state
            if (moveNumber != moveHistory.length + 1) {
                await refetchGame();
                return;
            }

            const decodedMove = decodeSingleMove(move);
            const decodedLegalMoves = decodeMovesIntoMap(legalMoves);
            setMoveHistory((last) => [...last, decodedMove]);

            chessboardRef.current?.makeMove(
                decodedMove,
                decodedLegalMoves,
                sideToMove,
            );
        },
    );

    async function sendMove(from: Point, to: Point) {
        await sendGameEvent("MovePieceAsync", gameToken, from, to);
    }

    return (
        <Chessboard
            {...chessboardProps}
            onPieceMovement={sendMove}
            ref={chessboardRef}
        />
    );
};
export default LiveChessboard;
