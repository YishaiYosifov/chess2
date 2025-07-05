import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { useGameEvent } from "@/features/signalR/hooks/useSignalRHubs";
import { Clocks, GameColor, getLiveGame } from "@/lib/apiClient";
import { decodeFen } from "@/lib/chessDecoders/fenDecoder";
import {
    decodeMoves,
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
        const { error, data } = await getLiveGame({ path: { gameToken } });
        if (error || !data) {
            console.error(error);
            return;
        }

        const pieces = decodeFen(data.fen);
        const legalMoves = decodeMovesIntoMap(data.legalMoves);
        chessboardStore
            .getState()
            .resetState(pieces, legalMoves, data.sideToMove);

        const moveHistory = decodeMoves(data.moveHistory);
        const { setMoveHistory } = liveChessStore.getState();
        setMoveHistory(moveHistory);
    }

    useGameEvent(
        gameToken,
        "MoveMadeAsync",
        async (
            move: string,
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

            const decodedMove = decodeSingleMove(move);
            receiveMove(decodedMove, clocks, sideToMove);

            if (sideToMove === playerColor)
                chessboardStore.getState().playMove(decodedMove);
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

    useGameEvent(
        gameToken,
        "GameEndedAsync",
        async (result, resultDescription, newWhiteRating, newBlackRating) => {
            liveChessStore
                .getState()
                .endGame(
                    result,
                    resultDescription,
                    newWhiteRating,
                    newBlackRating,
                );
            chessboardStore.getState().disableMovement();
            gameOverPopupRef.current?.open();
        },
    );
}
