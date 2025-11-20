import { StateCreator } from "zustand";

import type { ChessboardStore } from "./chessboardStore";
import { AnimationStep } from "../lib/types";

export interface AudioSlice {
    playAudioForAnimationStep(step: AnimationStep): Promise<void>;
}

export const createAudioSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    AudioSlice
> = (set) => ({
    async playAudioForAnimationStep(step) {
        if (step.movedPieceIds.length === 0) return;

        if (step.isCapture) {
            await new Audio("/assets/sfx/capture.webm").play();
        } else {
            await new Audio("/assets/sfx/move.webm").play();
        }
    },
});
