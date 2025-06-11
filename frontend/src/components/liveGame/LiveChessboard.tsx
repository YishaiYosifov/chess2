"use client";

import { Point } from "@/types/tempModels";
import Chessboard, { ChessboardProps } from "../game/Chessboard";
import { useGameEmitter } from "@/hooks/signalR/useSignalRHubs";

const LiveChessboard = ({
    gameToken,
    ...chessboardProps
}: { gameToken: string } & ChessboardProps) => {
    const sendGameEvent = useGameEmitter();

    async function sendMove(from: Point, to: Point) {
        await sendGameEvent("MovePieceAsync", gameToken, from, to);
    }

    return <Chessboard {...chessboardProps} onPieceMovement={sendMove} />;
};
export default LiveChessboard;
