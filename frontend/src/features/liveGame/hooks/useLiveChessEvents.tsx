import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { useGameEvent } from "@/features/signalR/hooks/useSignalRHubs";
import { Clocks, GameColor, getGame, MoveSnapshot } from "@/lib/apiClient";
import { decodeFen } from "@/lib/chessDecoders/fenDecoder";
import {
    decodeMovesIntoMap,
    decodeSingleMove,
} from "@/lib/chessDecoders/moveDecoder";
import { StoreApi } from "zustand";
import { LiveChessStore } from "../stores/liveChessStore";
import { GameOverPopupRef } from "../components/GameOverPopup";

export function useLiveChessEvents(
    gameToken: string,
    playerColor: GameColor,
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
    gameOverPopupRef: React.RefObject<GameOverPopupRef | null>,
) {
    async function refetchGame() {
        const { error, data } = await getGame({ path: { gameToken } });
        if (error || !data) {
            console.error(error);
            return;
        }

        const pieces = decodeFen(data.fen);
        const legalMoves = decodeMovesIntoMap(data.legalMoves);
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
                const decodedMove = decodeSingleMove(move.encodedMove);
                chessboardStore.getState().playMove(decodedMove);
            }
        },
    );

    useGameEvent(
        gameToken,
        "LegalMovesChangedAsync",
        async (legalMoves: string[]) => {
            const decodedLegalMoves = decodeMovesIntoMap(legalMoves);
            chessboardStore.getState().setLegalMoves(decodedLegalMoves);
        },
    );

    useGameEvent(gameToken, "GameEndedAsync", async (result) => {
        liveChessStore.getState().endGame(result);
        chessboardStore.getState().disableMovement();
        gameOverPopupRef.current?.open();
    });
}
