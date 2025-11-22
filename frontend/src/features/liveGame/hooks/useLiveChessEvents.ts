import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { Clocks, GameColor, MoveSnapshot } from "@/lib/apiClient";
import { StoreApi, useStore } from "zustand";
import { LiveChessStore } from "../stores/liveChessStore";
import { decodePath, decodeEncodedMovesIntoMap } from "../lib/moveDecoder";
import { Position } from "../lib/types";
import { ProcessedMoveOptions } from "@/features/chessboard/lib/types";
import { refetchGame } from "../lib/gameStateProcessor";
import { useGameEvent } from "./useGameHub";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";

export default function useLiveChessEvents(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const boardDimensions = useStore(chessboardStore, (x) => x.boardDimensions);
    const gameToken = useStore(liveChessStore, (x) => x.gameToken);

    async function jumpForwards() {
        const { positionHistory, viewingMoveNumber, teleportToLastMove } =
            liveChessStore.getState();
        const { goToPosition } = chessboardStore.getState();
        if (viewingMoveNumber !== positionHistory.length - 1) {
            const position = teleportToLastMove();
            await goToPosition(position.state);
        }
    }

    useGameEvent(gameToken, "SyncRevisionAsync", async (currentRevision) => {
        const { sourceRevision } = liveChessStore.getState();
        if (sourceRevision !== currentRevision) {
            await refetchGame(liveChessStore, chessboardStore);
        }
    });

    useGameEvent(
        gameToken,
        "MoveMadeAsync",
        async (
            move: MoveSnapshot,
            sideToMove: GameColor,
            moveNumber: number,
            clocks: Clocks,
        ) => {
            const {
                positionHistory,
                isPendingMoveAck,
                viewer,
                receiveMove,
                resetLegalMovesForOpponentTurn,
            } = liveChessStore.getState();
            const { applyMoveWithIntermediates, disableMovement } =
                chessboardStore.getState();

            // we missed a move... we need to refetch the state
            if (moveNumber != positionHistory.length) {
                await refetchGame(liveChessStore, chessboardStore);
                return;
            }

            if (viewer.playerColor !== sideToMove) {
                disableMovement();
                resetLegalMovesForOpponentTurn();
            }

            const decodedMove = decodePath(move.path, boardDimensions.width);
            if (!isPendingMoveAck) {
                await jumpForwards();
                await applyMoveWithIntermediates(decodedMove);
            }

            const pieces = chessboardStore.getState().pieces;
            const position: Position = {
                san: move.san,
                move: decodedMove,
                pieces,
                clocks: {
                    whiteClock: clocks.whiteClock,
                    blackClock: clocks.blackClock,
                },
            };
            receiveMove(position, clocks, sideToMove);
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

    useGameEvent(gameToken, "DrawStateChangeAsync", (drawState) =>
        liveChessStore.getState().drawStateChange(drawState),
    );

    useGameEvent(gameToken, "GameEndedAsync", async (result, finalClocks) => {
        liveChessStore.getState().endGame(result, finalClocks);
        chessboardStore.getState().disableMovement();
        AudioPlayer.playAudio(AudioType.GAME_END);
    });
}
