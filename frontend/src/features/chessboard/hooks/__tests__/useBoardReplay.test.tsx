import { StoreApi } from "zustand";
import {
    ChessboardStore,
    createChessboardStore,
} from "../../stores/chessboardStore";
import { act, renderHook } from "@testing-library/react";
import useBoardReplay from "../useBoardReplay";
import expandMinimalMove from "../../lib/expandMinimalMove";
import { decodeFen } from "../../lib/fenDecoder";
import { GameReplay } from "../../lib/types";
import { logicalPoint } from "@/features/point/pointUtils";
import { simulateMove } from "../../lib/simulateMove";
import mockSequentialUUID from "@/lib/testUtils/mocks/mockUuids";
import BoardPieces from "../../lib/boardPieces";

describe("useBoardReplay", () => {
    let chessboardStore: StoreApi<ChessboardStore>;
    let replays: GameReplay[];

    beforeEach(() => {
        chessboardStore = createChessboardStore();
        vi.useFakeTimers();

        replays = [
            {
                startingFen:
                    "rhnbqkbcar/pppdppdppp/10/10/9+/+9/10/10/PPPDPPDPPP/RHNBQKBCAR",
                moves: [
                    {
                        from: logicalPoint({ x: 4, y: 1 }),
                        to: logicalPoint({ x: 4, y: 4 }),
                    }, // e5
                    {
                        from: logicalPoint({ x: 4, y: 8 }),
                        to: logicalPoint({ x: 4, y: 5 }),
                    }, // e6
                ],
            },
            {
                startingFen: "b9/10/10/10/10/10/10/10/10/B9",
                moves: [
                    {
                        from: logicalPoint({ x: 0, y: 0 }),
                        to: logicalPoint({ x: 9, y: 9 }),
                    },
                    {
                        from: logicalPoint({ x: 0, y: 9 }),
                        to: logicalPoint({ x: 9, y: 0 }),
                    },
                ],
            },
        ];
    });

    function advanceReplay(replay: GameReplay) {
        for (let i = 0; i < replay.moves.length; i++) {
            act(() => vi.advanceTimersByTime(1000));
        }
    }

    it("should set initial board position from starting FEN", () => {
        mockSequentialUUID();
        renderHook(() => useBoardReplay(replays, chessboardStore));

        mockSequentialUUID();
        expect(chessboardStore.getState().pieces).toEqual(
            decodeFen(replays[0].startingFen),
        );
    });

    it("should apply moves sequentially every 1 second", () => {
        mockSequentialUUID();
        renderHook(() => useBoardReplay(replays, chessboardStore));

        let nextPieces = new BoardPieces(chessboardStore.getState().pieces);
        nextPieces = simulateMove(
            nextPieces,
            expandMinimalMove(replays[0].moves[0]),
        ).newPieces;

        mockSequentialUUID();
        expect(chessboardStore.getState().pieces).toEqual(
            decodeFen(replays[0].startingFen),
        );

        act(() => vi.advanceTimersByTime(1000));
        expect(chessboardStore.getState().pieces).toEqual(nextPieces);

        act(() => vi.advanceTimersByTime(500));
        // unchanged until 1 second passes
        expect(chessboardStore.getState().pieces).toEqual(nextPieces);
        act(() => vi.advanceTimersByTime(500));

        nextPieces = simulateMove(
            nextPieces,
            expandMinimalMove(replays[0].moves[1]),
        ).newPieces;
        expect(chessboardStore.getState().pieces).toEqual(nextPieces);
    });

    it("should advance to next replay after last move", () => {
        mockSequentialUUID();
        renderHook(() => useBoardReplay(replays, chessboardStore));

        mockSequentialUUID();
        advanceReplay(replays[0]);
        act(() => vi.advanceTimersByTime(2000));

        mockSequentialUUID();
        expect(chessboardStore.getState().pieces).toEqual(
            decodeFen(replays[1].startingFen),
        );
    });

    it("should loop back to first replay after the last replay", () => {
        mockSequentialUUID();
        renderHook(() => useBoardReplay(replays, chessboardStore));

        mockSequentialUUID();
        advanceReplay(replays[0]);
        act(() => vi.advanceTimersByTime(2000));

        mockSequentialUUID();
        advanceReplay(replays[1]);
        act(() => vi.advanceTimersByTime(2000));

        mockSequentialUUID();
        expect(chessboardStore.getState().pieces).toEqual(
            decodeFen(replays[0].startingFen),
        );
    });

    it("should clear timeout on unmount", () => {
        mockSequentialUUID();
        const { unmount } = renderHook(() =>
            useBoardReplay(replays, chessboardStore),
        );

        unmount();
        mockSequentialUUID();
        act(() => vi.advanceTimersByTime(5000));

        mockSequentialUUID();
        expect(chessboardStore.getState().pieces).toEqual(
            decodeFen(replays[0].startingFen),
        );
    });
});
