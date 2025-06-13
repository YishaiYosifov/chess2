"use client";

import { Move, Point } from "@/types/tempModels";
import Chessboard, { ChessboardProps, ChessboardRef } from "../game/Chessboard";
import { useGameEmitter, useGameEvent } from "@/hooks/signalR/useSignalRHubs";
import { useRef, useState } from "react";
import { useRouter } from "next/navigation";

const LiveChessboard = ({
    gameToken,
    ...chessboardProps
}: { gameToken: string } & ChessboardProps) => {
    const router = useRouter();
    const chessboardRef = useRef<ChessboardRef>(null);
    const [moveHistory, setMoveHistory] = useState<Move[]>([]);

    const sendGameEvent = useGameEmitter(gameToken);
    useGameEvent(
        gameToken,
        "MoveMadeAsync",
        (...args) => location.reload(),
        //chessboardRef?.current?.makeMove(...args),
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
