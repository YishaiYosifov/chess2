import { StateCreator } from "zustand";

import type { ChessboardStore } from "./chessboardStore";
import { SpecialMoveType } from "@/lib/apiClient";
import { AnimationStep } from "../lib/types";
import AudioPlayer, { AudioType } from "../../audio/audioPlayer";

export interface AudioSliceProps {
    muteAudio: boolean;
}

export interface AudioSlice {
    muteAudio: boolean;
    playAudioForAnimationStep(step: AnimationStep): Promise<void>;
}

const SPECIAL_MOVE_AUDIO_MAP: Partial<Record<SpecialMoveType, AudioType>> = {
    [SpecialMoveType.KNOOKLEAR_FUSION]: AudioType.KNOOKLEAR_FUSION,
    [SpecialMoveType.KINGSIDE_CASTLE]: AudioType.CASTLE,
    [SpecialMoveType.QUEENSIDE_CASTLE]: AudioType.CASTLE,
    [SpecialMoveType.VERTICAL_CASTLE]: AudioType.CASTLE,
    [SpecialMoveType.IL_VATICANO]: AudioType.CASTLE,
};

export function createAudioSlice(
    initState: AudioSliceProps,
): StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    AudioSlice
> {
    return (_, get) => ({
        ...initState,
        cachedAudios: new Map(),

        async playAudioForAnimationStep(step) {
            const { muteAudio } = get();
            if (muteAudio || step.movedPieceIds.length === 0) return;

            const specialMoveAudio = step.specialMoveType
                ? SPECIAL_MOVE_AUDIO_MAP[step.specialMoveType]
                : null;
            if (specialMoveAudio) {
                await AudioPlayer.playAudio(specialMoveAudio);
                return;
            }

            if (step.isPromotion)
                await AudioPlayer.playAudio(AudioType.PROMOTION);

            if (step.isCapture) await AudioPlayer.playAudio(AudioType.CAPTURE);
            else await AudioPlayer.playAudio(AudioType.MOVE);
        },
    });
}
