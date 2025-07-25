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
import { Position, ProcessedMoveOptions } from "@/types/tempModels";

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
        chessboardStore.getState().resetState(pieces, legalMoves);

        const { setMoveHistory } = liveChessStore.getState();
        setMoveHistory(data.moveHistory);
    }

    function jumpForwards() {
        const { positionHistory, viewingMoveNumber, teleportToLastMove } =
            liveChessStore.getState();
        const { setPosition } = chessboardStore.getState();
        if (viewingMoveNumber !== positionHistory.length - 1) {
            const position = teleportToLastMove();
            setPosition(position);
        }
    }

    function addCurrentPosition(
        move: MoveSnapshot,
        clocks: Clocks,
        sideToMove: GameColor,
    ) {
        const { receiveMove } = liveChessStore.getState();

        const pieces = chessboardStore.getState().pieces;
        const position: Position = {
            san: move.san,
            pieces,
            clocks: {
                whiteClock: clocks.whiteClock,
                blackClock: clocks.blackClock,
            },
        };
        receiveMove(position, clocks, sideToMove);
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
            const { positionHistory } = liveChessStore.getState();
            const { applyMove } = chessboardStore.getState();

            // we missed a move... we need to refetch the state
            if (moveNumber != positionHistory.length) {
                await refetchGame();
                return;
            }

            if (sideToMove === playerColor) {
                jumpForwards();
                const decoded = decodePath(move.path, boardDimensions.width);
                applyMove(decoded);
                addCurrentPosition(move, clocks, sideToMove);
            } else {
                addCurrentPosition(move, clocks, sideToMove);
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

            const moveOptions: ProcessedMoveOptions = {
                legalMoves: decodedLegalMoves,
                hasForcedMoves,
            };
            liveChessStore.getState().receiveLegalMoves(moveOptions);
            chessboardStore.getState().setLegalMoves(moveOptions);
        },
    );

    useGameEvent(gameToken, "GameEndedAsync", async (result) => {
        liveChessStore.getState().endGame(result);
        chessboardStore.getState().disableMovement();
        gameOverPopupRef.current?.open();
    });
}
