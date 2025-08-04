import { getGame, type GameState } from "@/lib/apiClient";
import { LiveChessStore, LiveChessStoreProps } from "../stores/liveChessStore";
import {
    ChessboardProps,
    ChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import { decodeFen } from "./fenDecoder";
import { ClockSnapshot } from "./types";
import { Position } from "./types";
import { ProcessedMoveOptions } from "@/features/chessboard/lib/types";
import { decodePath, decodePathIntoMap } from "./moveDecoder";
import { simulateMove } from "@/features/chessboard/lib/simulateMove";
import constants from "@/lib/constants";
import { StoreApi } from "zustand";

export interface ProcessedGameState {
    live: LiveChessStoreProps;
    board: ChessboardProps;
}

export function createStoreProps(
    gameToken: string,
    userId: string,
    gameState: GameState,
): ProcessedGameState {
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
        userId === gameState.whitePlayer.userId
            ? gameState.whitePlayer.color
            : gameState.blackPlayer.color;

    const live: LiveChessStoreProps = {
        gameToken,

        whitePlayer: gameState.whitePlayer,
        blackPlayer: gameState.blackPlayer,
        sideToMove: gameState.sideToMove,

        userId,
        playerColor,

        positionHistory,
        viewingMoveNumber: positionHistory.length - 1,
        latestMoveOptions,

        drawState: gameState.drawState,
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

function getPositionHistory(gameState: GameState): Position[] {
    let pieces = decodeFen(gameState.initialFen);

    const baseClock = gameState.timeControl.baseSeconds * 1000;
    let clockSnapshot: ClockSnapshot = {
        whiteClock: baseClock,
        blackClock: baseClock,
    };
    const positionHistory: Position[] = [
        {
            pieces,
            clocks: { ...clockSnapshot },
        },
    ];
    for (const [i, moveSnapshot] of gameState.moveHistory.entries()) {
        clockSnapshot = {
            whiteClock:
                i % 2 === 0 ? moveSnapshot.timeLeft : clockSnapshot.whiteClock,
            blackClock:
                i % 2 !== 0 ? moveSnapshot.timeLeft : clockSnapshot.blackClock,
        };

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

export async function refetchGame(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const { gameToken, userId } = liveChessStore.getState();

    const { error, data: gameState } = await getGame({
        path: { gameToken },
    });
    if (error || !gameState) {
        console.error(error);
        return;
    }

    const { live, board } = createStoreProps(gameToken, userId, gameState);
    liveChessStore.getState().resetState(live);
    chessboardStore.getState().resetState(board);
}
