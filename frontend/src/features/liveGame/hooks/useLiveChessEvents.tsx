import { ChessboardState } from "@/features/chessboard/stores/chessboardStore";
import { useGameEvent } from "@/features/signalR/hooks/useSignalRHubs";
import { Clocks, GameColor, getGame, MoveSnapshot } from "@/lib/apiClient";
import { decodeFen } from "../lib/fenDecoder";
import { StoreApi, useStore } from "zustand";
import { LiveChessStore } from "../stores/liveChessStore";
import { GameOverPopupRef } from "../components/GameOverPopup";
import {
    decodePath,
    decodePathIntoMap,
    decodeEncodedMovesIntoMap,
} from "../lib/moveDecoder";

export function useLiveChessEvents(
    gameToken: string,
    playerColor: GameColor,
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardState>,
    gameOverPopupRef: React.RefObject<GameOverPopupRef | null>,
) {
    const boardDimensions = useStore(chessboardStore, (x) => x.boardDimensions);

    async function refetchGame() {
        const { error, data } = await getGame({ path: { gameToken } });
        if (error || !data) {
            console.error(error);
            return;
        }

        const pieces = decodeFen(data.fen);
        const legalMoves = decodePathIntoMap(
            data.legalMoves,
            boardDimensions.width,
        );
        chessboardStore
            .getState()
            .resetState(pieces, legalMoves, data.sideToMove);

        const { setMoveHistory } = liveChessStore.getState();
        setMoveHistory(data.moveHistory);
    }

    useGameEvent(
        gameToken,
        "MoveMadeAsync",
        async (
            move: MoveSnapshot,
            sideToMove: GameColor,
            moveNumber: number,
            clocks: Clocks,
        ) => {
            const { moveHistory, receiveMove } = liveChessStore.getState();
            // we missed a move... we need to refetch the state
            if (moveNumber != moveHistory.length + 1) {
                await refetchGame();
                return;
            }

            receiveMove(move, clocks, sideToMove);

            if (sideToMove === playerColor) {
                const decoded = decodePath(move.path, boardDimensions.width);
                chessboardStore.getState().applyMove(decoded);
            }
        },
    );

    useGameEvent(
        gameToken,
        "LegalMovesChangedAsync",
        async (legalMoves, hasForcedMoves) => {
            const decodedLegalMoves = decodeEncodedMovesIntoMap(
                legalMoves,
                boardDimensions.width,
            );

            chessboardStore
                .getState()
                .setLegalMoves(decodedLegalMoves, hasForcedMoves);
        },
    );

    useGameEvent(gameToken, "GameEndedAsync", async (result) => {
        liveChessStore.getState().endGame(result);
        chessboardStore.getState().disableMovement();
        gameOverPopupRef.current?.open();
    });
}
