"use client";

import { Point } from "@/types/tempModels";
import Chessboard, { ChessboardProps, ChessboardRef } from "../game/Chessboard";
import { useGameEmitter, useGameEvent } from "@/hooks/signalR/useSignalRHubs";
import { useRef } from "react";

const LiveChessboard = ({
    gameToken,
    ...chessboardProps
}: { gameToken: string } & ChessboardProps) => {
    const chessboardRef = useRef<ChessboardRef>(null);

    const sendGameEvent = useGameEmitter(gameToken);
    // useGameEvent(
    //     gameToken,
    //     "MoveMadeAsync",
    //     (...args) => location.reload(),
    //     //chessboardRef?.current?.makeMove(...args),
    // );

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
