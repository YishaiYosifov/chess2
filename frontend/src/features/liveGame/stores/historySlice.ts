import { StateCreator } from "zustand";

import {
    BoardState,
    ProcessedMoveOptions,
} from "@/features/chessboard/lib/types";

import { createMoveOptions } from "@/features/chessboard/lib/moveOptions";
import type { LiveChessStore } from "./liveChessStore";
import { HistoryStep, Position } from "../lib/types";

export interface HistorySliceProps {
    positionHistory: Position[];
    viewingMoveNumber: number;
    latestMoveOptions: ProcessedMoveOptions;
}

export interface HistorySlice extends HistorySliceProps {
    teleportToMove(number: number): HistoryStep | undefined;
    shiftMoveViewBy(amount: number): HistoryStep | undefined;
    teleportToLastMove(): HistoryStep;

    receivePosition(position: Position): void;
}

export function createHistorySlice(
    initState: HistorySliceProps,
): StateCreator<
    LiveChessStore,
    [["zustand/immer", never], never],
    [],
    HistorySlice
> {
    return (set, get) => ({
        ...initState,

        teleportToMove(number) {
            const { positionHistory, latestMoveOptions, viewingMoveNumber } =
                get();
            if (number < 0 || number >= positionHistory.length) return;

            const isLatestPosition = number === positionHistory.length - 1;
            const position = positionHistory[number];
            const isOneStepForward = number === viewingMoveNumber + 1;

            set((state) => {
                state.viewingMoveNumber = number;
            });

            const state: BoardState = {
                moveOptions: isLatestPosition
                    ? latestMoveOptions
                    : createMoveOptions(),
                pieces: position.pieces,
                moveThatProducedPosition: position.move,
                // the move that should be considered "last" from the perspective of the current viewed position.
                // if moving forward in history (number > viewingMoveNumber), this is the move that produced the current position (position.move).
                // if moving backward in history (number < viewingMoveNumber), this is the move in the next position,
                // because that was the move that brought us to the current position from the previous step.
                moveFromPreviousViewedPosition:
                    number > viewingMoveNumber
                        ? position.move
                        : positionHistory[number + 1]?.move,
            };

            return {
                state,
                isOneStepForward,
            };
        },

        shiftMoveViewBy(amount) {
            const { teleportToMove, viewingMoveNumber } = get();
            return teleportToMove(viewingMoveNumber + amount);
        },

        teleportToLastMove() {
            const { positionHistory, teleportToMove } = get();
            const lastIndex = positionHistory.length - 1;
            if (lastIndex < 0) throw new Error("positionHistory is empty");
            return teleportToMove(lastIndex)!;
        },

        receivePosition(position) {
            const { viewingMoveNumber, positionHistory } = get();
            set((state) => {
                if (viewingMoveNumber === positionHistory.length - 1)
                    state.viewingMoveNumber++;

                state.positionHistory.push(position);
            });
        },
    });
}
