import type { GameState } from "@/lib/apiClient";
import { LiveChessStoreProps } from "../stores/liveChessStore";
import { ChessboardProps } from "@/features/chessboard/stores/chessboardStore";
import { decodeFen } from "./fenDecoder";
import {
    ClockSnapshot,
    Position,
    ProcessedMoveOptions,
} from "@/types/tempModels";
import { decodePath, decodePathIntoMap } from "./moveDecoder";
import { simulateMove } from "@/features/chessboard/lib/simulateMove";
import constants from "@/lib/constants";

export function createStoreProps(
    gameToken: string,
    userId: string,
    gameState: GameState,
): {
    live: LiveChessStoreProps;
    board: ChessboardProps;
} {
    const positionHistory = getPositionHistory(gameState);
    const boardWidth = constants.BOARD_WIDTH;
    const boardHeight = constants.BOARD_HEIGHT;
    const legalMoves = decodePathIntoMap(
        gameState.moveOptions.legalMoves,
        boardWidth,
    );
    const latestMoveOptions: ProcessedMoveOptions = {
        legalMoves,
        hasForcedMoves: gameState.moveOptions.hasForcedMoves,
    };

    const playerColor =
        userId == gameState.whitePlayer.userId
            ? gameState.whitePlayer.color
            : gameState.blackPlayer.color;

    const live: LiveChessStoreProps = {
        gameToken,

        whitePlayer: gameState.whitePlayer,
        blackPlayer: gameState.blackPlayer,
        playerColor,
        sideToMove: gameState.sideToMove,

        positionHistory,
        viewingMoveNumber: positionHistory.length - 1,
        latestMoveOptions,
        clocks: gameState.clocks,
        resultData: gameState.resultData ?? null,
    };
    const board: ChessboardProps = {
        pieces: positionHistory.at(-1)?.pieces ?? new Map(),
        moveOptions: latestMoveOptions,

        boardDimensions: { width: boardWidth, height: boardHeight },
        viewingFrom: playerColor,
    };

    return { live, board };
}

function getPositionHistory(gameState: GameState) {
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
