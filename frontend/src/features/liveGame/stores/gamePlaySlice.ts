import { StateCreator } from "zustand";

import { createMoveOptions } from "@/features/chessboard/lib/moveOptions";
import { ProcessedMoveOptions } from "@/features/chessboard/lib/types";
import { Clocks, GameColor, PoolKey } from "@/lib/apiClient";
import type { LiveChessStore } from "./liveChessStore";
import { Position } from "../lib/types";

export interface LiveChessViewer {
    userId: string;
    playerColor: GameColor | null;
}

export interface GamePlaySliceProps {
    sideToMove: GameColor;
    clocks: Clocks;

    viewer: LiveChessViewer;
    pool: PoolKey;
}

export interface GamePlaySlice extends GamePlaySliceProps {
    isPendingMoveAck: boolean;

    receiveMove(
        position: Position,
        clocks: Clocks,
        sideToMove: GameColor,
    ): void;
    resetLegalMovesForOpponentTurn(): void;
    receiveLegalMoves(moveOptions: ProcessedMoveOptions): void;
    markPendingMoveAck(): void;
}

export function createGamePlaySlice(
    initState: GamePlaySliceProps,
): StateCreator<
    LiveChessStore,
    [["zustand/immer", never], never],
    [],
    GamePlaySlice
> {
    return (set, get) => ({
        ...initState,

        isPendingMoveAck: false,

        receiveMove(position, clocks, sideToMove) {
            const { decrementDrawCooldown, receivePosition } = get();

            decrementDrawCooldown();
            receivePosition(position);
            set((state) => {
                state.clocks = clocks;
                state.sideToMove = sideToMove;
                state.isPendingMoveAck = false;
            });
        },
        resetLegalMovesForOpponentTurn() {
            set((state) => {
                state.latestMoveOptions = createMoveOptions();
            });
        },
        receiveLegalMoves(moveOptions) {
            set((state) => {
                state.latestMoveOptions = moveOptions;
            });
        },

        markPendingMoveAck() {
            set((state) => {
                state.isPendingMoveAck = true;
            });
        },
    });
}
