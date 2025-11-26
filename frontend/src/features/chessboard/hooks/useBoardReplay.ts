import { useEffect, useState } from "react";
import { GameReplay } from "../lib/types";
import { StoreApi } from "zustand";
import { ChessboardStore } from "../stores/chessboardStore";
import expandMinimalMove from "../lib/expandMinimalMove";
import { createMoveOptions } from "../lib/moveOptions";
import { decodeFen } from "../lib/fenDecoder";

export default function useBoardReplay(
    replays: GameReplay[],
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const [replayIndexRef, setReplayIndex] = useState(0);
    const [moveIndex, setMoveIndex] = useState(0);

    useEffect(() => {
        if (!replays.length) return;

        const currentReplay = replays[replayIndexRef];
        if (moveIndex === 0) {
            const pieces = decodeFen(currentReplay.startingFen);
            chessboardStore
                .getState()
                .goToPosition({ pieces, moveOptions: createMoveOptions() });
        }

        let timeout: NodeJS.Timeout;
        if (moveIndex >= currentReplay.moves.length) {
            timeout = setTimeout(() => {
                setReplayIndex((idx) => (idx + 1) % replays.length);
                setMoveIndex(0);
            }, 2000);
        } else {
            timeout = setTimeout(() => {
                const move = currentReplay.moves[moveIndex];
                chessboardStore
                    .getState()
                    .applyMoveAnimated(expandMinimalMove(move));
                setMoveIndex((idx) => idx + 1);
            }, 1000);
        }

        return () => clearTimeout(timeout);
    }, [moveIndex, replayIndexRef, replays, chessboardStore]);
}
