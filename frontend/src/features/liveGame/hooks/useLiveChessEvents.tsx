import { ChessboardState } from "@/features/chessboard/stores/chessboardStore";
import { useGameEvent } from "@/features/signalR/hooks/useSignalRHubs";
import { Clocks, GameColor, getGame, MoveSnapshot } from "@/lib/apiClient";
import { StoreApi, useStore } from "zustand";
import { LiveChessStore } from "../stores/liveChessStore";
import { GameOverPopupRef } from "../components/GameOverPopup";
import { decodePath, decodeEncodedMovesIntoMap } from "../lib/moveDecoder";
import { Position, ProcessedMoveOptions } from "@/types/tempModels";
import { createStoreProps } from "../lib/gameStateProcessor";

export function useLiveChessEvents(
    gameToken: string,
    userId: string,
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardState>,
    gameOverPopupRef: React.RefObject<GameOverPopupRef | null>,
) {
    const boardDimensions = useStore(chessboardStore, (x) => x.boardDimensions);

    async function refetchGame() {
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
            const { positionHistory, playerColor } = liveChessStore.getState();
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
