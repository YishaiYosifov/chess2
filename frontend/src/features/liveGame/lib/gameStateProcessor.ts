import {
    GameColor,
    GamePlayer,
    getGame,
    type GameState,
} from "@/lib/apiClient";
import { LiveChessStore, LiveChessStoreProps } from "../stores/liveChessStore";
import {
    ChessboardProps,
    ChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import { decodeFen } from "../../chessboard/lib/fenDecoder";
import { ClockSnapshot } from "./types";
import { Position } from "./types";
import { ProcessedMoveOptions } from "@/features/chessboard/lib/types";
import { decodePath, decodePathIntoMap } from "./moveDecoder";
import { simulateMove } from "@/features/chessboard/lib/simulateMove";
import constants from "@/lib/constants";
import { StoreApi } from "zustand";
import { LiveChessViewer } from "../stores/gamePlaySlice";

export interface ProcessedGameState {
    live: LiveChessStoreProps;
    board: ChessboardProps;
}

export function createStoreProps(
    gameToken: string,
    viewerUserId: string,
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

    const viewerColor = getViewerColor(
        gameState.whitePlayer,
        gameState.blackPlayer,
        viewerUserId,
    );
    const viewer: LiveChessViewer = {
        userId: viewerUserId,
        playerColor: viewerColor,
    };

    const live: LiveChessStoreProps = {
        gameToken,

        whitePlayer: gameState.whitePlayer,
        blackPlayer: gameState.blackPlayer,
        sideToMove: gameState.sideToMove,

        pool: gameState.pool,
        viewer,

        positionHistory,
        viewingMoveNumber: positionHistory.length - 1,
        latestMoveOptions,

        drawState: gameState.drawState,
        clocks: gameState.clocks,
        resultData: gameState.resultData ?? null,
    };
    const board: ChessboardProps = {
        pieceMap: positionHistory.at(-1)?.pieces ?? new Map(),
        moveOptions: latestMoveOptions,

        boardDimensions: { width: boardWidth, height: boardHeight },
        viewingFrom: viewerColor ?? GameColor.WHITE,
        canDrag: true,
    };

    return { live, board };
}

function getViewerColor(
    whitePlayer: GamePlayer,
    blackPlayer: GamePlayer,
    userId: string,
): GameColor | null {
    if (userId === whitePlayer.userId) return GameColor.WHITE;
    else if (userId === blackPlayer.userId) return GameColor.BLACK;
    return null;
}

function getPositionHistory(gameState: GameState): Position[] {
    let pieces = decodeFen(gameState.initialFen);

    const baseClock = gameState.pool.timeControl.baseSeconds * 1000;
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
            move,
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
    const {
        gameToken,
        viewer: { userId },
    } = liveChessStore.getState();

    const { error, data: gameState } = await getGame({
        path: { gameToken },
    });
    if (error || gameState === undefined) {
        console.error(error);
        return;
    }

    const { live, board } = createStoreProps(gameToken, userId, gameState);
    liveChessStore.getState().resetState(live);
    chessboardStore.getState().resetState(board);
}
