"use client";

import { GameState } from "@/lib/apiClient";
import { memo } from "react";
import LiveChessboard from "./LiveChessboard";
import { decodeFen } from "../lib/fenDecoder";
import { decodePath, decodePathIntoMap } from "../lib/moveDecoder";
import { simulateMove } from "@/features/chessboard/lib/simulateMove";
import constants from "@/lib/constants";
import {
    ClockSnapshot,
    Position,
    ProcessedMoveOptions,
} from "@/types/tempModels";

const GameStatePreprocessor = ({
    gameToken,
    gameState,
    userId,
}: {
    gameToken: string;
    gameState: GameState;
    userId: string;
}) => {
    function getPositionHistory() {
        let pieces = decodeFen(gameState.initialFen);

        const baseClock = gameState.timeControl.baseSeconds * 1000;
        const clockSnapshot: ClockSnapshot = {
            whiteClock: baseClock,
            blackClock: baseClock,
        };
        const positionHistory: Position[] = [
            {
                pieces,
                clocks: clockSnapshot,
            },
        ];
        for (const [i, moveSnapshot] of gameState.moveHistory.entries()) {
            if (i % 2 == 0) {
                clockSnapshot.whiteClock = moveSnapshot.timeLeft;
            } else {
                clockSnapshot.blackClock = moveSnapshot.timeLeft;
            }

            const move = decodePath(moveSnapshot.path, constants.BOARD_WIDTH);
            const { newPieces } = simulateMove(pieces, move);

            const position: Position = {
                san: moveSnapshot.san,
                pieces: newPieces,
                clocks: { ...clockSnapshot },
            };
            positionHistory.push(position);
            pieces = newPieces;
        }
        return positionHistory;
    }

    const positionHistory = getPositionHistory();
    const boardWidth = constants.BOARD_WIDTH;
    const boardHeight = constants.BOARD_HEIGHT;
    const legalMoves = decodePathIntoMap(
        gameState.moveOptions.legalMoves,
        boardWidth,
    );
    const moveOptions: ProcessedMoveOptions = {
        legalMoves,
        hasForcedMoves: gameState.moveOptions.hasForcedMoves,
    };

    const playerColor =
        userId == gameState.whitePlayer.userId
            ? gameState.whitePlayer.color
            : gameState.blackPlayer.color;

    return (
        <LiveChessboard
            gameToken={gameToken}
            playerColor={playerColor}
            positionHistory={positionHistory}
            moveOptions={moveOptions}
            boardDimensions={{ width: boardWidth, height: boardHeight }}
            gameState={gameState}
        />
    );
};
export default memo(GameStatePreprocessor);
