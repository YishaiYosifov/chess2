"use client";

import { Move, Point } from "@/types/tempModels";
import Chessboard, { ChessboardRef } from "../game/Chessboard";
import { useGameEmitter, useGameEvent } from "@/hooks/signalR/useSignalRHubs";
import { useMemo, useRef, useState } from "react";
import {
    decodeMoves,
    decodeMovesIntoMap,
    decodeSingleMove,
} from "@/lib/chessDecoders/moveDecoder";
import { GameColor, GameState, getLiveGame } from "@/lib/apiClient";
import { decodeFen } from "@/lib/chessDecoders/fenDecoder";
import ProfilePicture from "../profile/ProfilePicture";

const LiveChessboard = ({
    gameToken,
    gameState,
    userId,
}: {
    gameToken: string;
    gameState: GameState;
    userId: string;
}) => {
    const chessboardRef = useRef<ChessboardRef>(null);

    const decodedLegalMoves = useMemo(
        () => decodeMovesIntoMap(gameState.legalMoves),
        [gameState.legalMoves],
    );
    const decodedMoveHistory = useMemo(
        () => decodeMoves(gameState.moveHistory),
        [gameState.moveHistory],
    );
    const decodedFen = useMemo(() => decodeFen(gameState.fen), [gameState.fen]);

    const playingAs =
        userId == gameState.playerWhite.userId
            ? gameState.playerWhite
            : gameState.playerBlack;

    const [moveHistory, setMoveHistory] = useState<Move[]>(decodedMoveHistory);

    async function refetchGame() {
        const { error, data } = await getLiveGame({ path: { gameToken } });
        if (error || !data) {
            console.error(error);
            return;
        }

        const pieces = decodeFen(data.fen);
        const legalMoves = decodeMovesIntoMap(data.legalMoves);
        chessboardRef.current?.resetState(pieces, legalMoves, data.sideToMove);

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
            // we missed a move... we need to refetch the state
            if (moveNumber != moveHistory.length + 1) {
                await refetchGame();
                return;
            }

            const decodedMove = decodeSingleMove(move);
            const decodedLegalMoves = decodeMovesIntoMap(legalMoves);
            setMoveHistory((last) => [...last, decodedMove]);

            chessboardRef.current?.playTurn(
                decodedLegalMoves,
                sideToMove,
                sideToMove == playingAs.color ? decodedMove : undefined,
            );
        },
    );

    async function sendMove(from: Point, to: Point) {
        await sendGameEvent("MovePieceAsync", gameToken, from, to);
    }

    return (
        <div className="flex flex-col gap-3">
            {/* <ProfilePicture height={50} width={50} /> */}
            <Chessboard
                ref={chessboardRef}
                onPieceMovement={sendMove}
                startingPieces={decodedFen}
                legalMoves={decodedLegalMoves}
                playingAs={playingAs.color}
                sideToMove={gameState.sideToMove}
                breakpoints={[
                    {
                        maxScreenSize: 768,
                        paddingOffset: { width: 40, height: 110 },
                    },
                    {
                        maxScreenSize: 1024,
                        paddingOffset: { width: 200, height: 50 },
                    },
                ]}
                defaultOffset={{ width: 626, height: 100 }}
            />
            {/* <ProfilePicture height={50} width={50} /> */}
        </div>
    );
};
export default LiveChessboard;
