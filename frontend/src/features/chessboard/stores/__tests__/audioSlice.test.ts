import { StoreApi } from "zustand";

import { ChessboardStore, createChessboardStore } from "../chessboardStore";
import { mockAudio } from "@/lib/testUtils/mocks/mockAudio";
import { AudioType } from "@/features/audio/audioPlayer";
import { SpecialMoveType } from "@/lib/apiClient";
import { AnimationStep } from "../../lib/types";
import BoardPieces from "../../lib/boardPieces";

describe("AudioSlice", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        store = createChessboardStore();
    });

    it("should not play audio if movedPieceIds is empty", async () => {
        const { audioMock, audioConstructorMock } = mockAudio();

        const step: AnimationStep = {
            newPieces: new BoardPieces(),
            movedPieceIds: [],
        };

        await store.getState().playAudioForAnimationStep(step);

        expect(audioConstructorMock).not.toHaveBeenCalled();
        expect(audioMock.play).not.toHaveBeenCalled();
    });

    it("should not play audio if muteAudio is true", async () => {
        const { audioMock, audioConstructorMock } = mockAudio();
        store.setState({ muteAudio: true });
        const step: AnimationStep = {
            newPieces: new BoardPieces(),
            movedPieceIds: ["1"],
        };

        await store.getState().playAudioForAnimationStep(step);

        expect(audioConstructorMock).not.toHaveBeenCalled();
        expect(audioMock.play).not.toHaveBeenCalled();
    });

    it("should play special move audio if step has specialMoveType", async () => {
        const { audioMock, audioConstructorMock } = mockAudio();
        const step: AnimationStep = {
            newPieces: new BoardPieces(),
            movedPieceIds: ["1"],
            isCapture: true,
            specialMoveType: SpecialMoveType.KNOOKLEAR_FUSION,
        };

        await store.getState().playAudioForAnimationStep(step);

        expect(audioConstructorMock).toHaveBeenCalledExactlyOnceWith(
            AudioType.KNOOKLEAR_FUSION,
        );
        expect(audioMock.play).toHaveBeenCalledOnce();
    });

    it("should play capture audio if isCapture is true and no special move", async () => {
        const { audioMock, audioConstructorMock } = mockAudio();
        const step: AnimationStep = {
            newPieces: new BoardPieces(),
            movedPieceIds: ["1"],
            isCapture: true,
        };

        await store.getState().playAudioForAnimationStep(step);

        expect(audioConstructorMock).toHaveBeenCalledExactlyOnceWith(
            AudioType.CAPTURE,
        );
        expect(audioMock.play).toHaveBeenCalledOnce();
    });

    it("should play normal move audio if no capture or special move", async () => {
        const { audioMock, audioConstructorMock } = mockAudio();
        const step: AnimationStep = {
            newPieces: new BoardPieces(),
            movedPieceIds: ["1"],
        };

        await store.getState().playAudioForAnimationStep(step);

        expect(audioConstructorMock).toHaveBeenCalledExactlyOnceWith(
            AudioType.MOVE,
        );
        expect(audioMock.play).toHaveBeenCalledOnce();
    });

    it("should play both promotion and move audio when promotion", async () => {
        const { audioMock, audioConstructorMock } = mockAudio();
        const step: AnimationStep = {
            newPieces: new BoardPieces(),
            movedPieceIds: ["1"],
            isPromotion: true,
        };

        await store.getState().playAudioForAnimationStep(step);

        expect(audioConstructorMock).toHaveBeenCalledWith(AudioType.PROMOTION);
        expect(audioConstructorMock).toHaveBeenCalledWith(AudioType.MOVE);
        expect(audioMock.play).toBeCalledTimes(2);
    });
});
