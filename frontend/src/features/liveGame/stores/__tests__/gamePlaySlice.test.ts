import { StoreApi } from "zustand";
import createLiveChessStore, {
    LiveChessStore,
    LiveChessStoreProps,
} from "../liveChessStore";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { Clocks, GameColor } from "@/lib/apiClient";
import { createFakePosition } from "@/lib/testUtils/fakers/positionFaker";
import { createMoveOptions } from "@/features/chessboard/lib/moveOptions";
import { createFakeLegalMoveMap } from "@/lib/testUtils/fakers/chessboardFakers";
import {
    LegalMoveMap,
    ProcessedMoveOptions,
} from "@/features/chessboard/lib/types";
import { createFakeClock } from "@/lib/testUtils/fakers/clockFaker";

describe("gamePlaySlice", () => {
    let store: StoreApi<LiveChessStore>;
    let initialProps: LiveChessStoreProps;

    beforeEach(() => {
        initialProps = createFakeLiveChessStoreProps();
        store = createLiveChessStore(initialProps);
    });

    describe("receiveMove", () => {
        it("should update clocks, sideToMove, and clear isPendingMoveAck", () => {
            const newPosition = createFakePosition();
            const newClocks: Clocks = {
                whiteClock: 500,
                blackClock: 600,
                lastUpdated: Date.now().valueOf(),
                isFrozen: true,
            };
            const newSideToMove = GameColor.BLACK;

            store.setState({
                isPendingMoveAck: true,
                clocks: {
                    whiteClock: 100,
                    blackClock: 200,
                    lastUpdated: Date.now().valueOf(),
                    isFrozen: false,
                },
                sideToMove: GameColor.WHITE,
            });

            store.getState().receiveMove(newPosition, newClocks, newSideToMove);
            const state = store.getState();

            expect(state.clocks).toBe(newClocks);
            expect(state.sideToMove).toBe(newSideToMove);
            expect(state.isPendingMoveAck).toBe(false);
        });

        it("should call receivePosition and decrementDrawCooldown", () => {
            const decrementMock = vi.fn();
            const receivePosSpy = vi.fn();

            store.setState({
                decrementDrawCooldown: decrementMock,
                receivePosition: receivePosSpy,
            });

            const newPosition = createFakePosition();
            store
                .getState()
                .receiveMove(newPosition, createFakeClock(), GameColor.WHITE);

            expect(decrementMock).toHaveBeenCalledOnce();
            expect(receivePosSpy).toHaveBeenCalledExactlyOnceWith(newPosition);
        });
    });

    describe("resetLegalMovesForOpponentTurn", () => {
        it("should reset latestMoveOptions", () => {
            const testMoveOptions = createMoveOptions({
                legalMoves: createFakeLegalMoveMap(),
            });
            store.setState({ latestMoveOptions: testMoveOptions });

            store.getState().resetLegalMovesForOpponentTurn();

            expect(store.getState().latestMoveOptions).toEqual(
                createMoveOptions(),
            );
        });
    });

    describe("receiveLegalMoves", () => {
        it("should update latestMoveOptions", () => {
            const newMoves: LegalMoveMap = createFakeLegalMoveMap();
            const moveOptions: ProcessedMoveOptions = {
                legalMoves: newMoves,
                hasForcedMoves: true,
            };

            store.getState().receiveLegalMoves(moveOptions);
            expect(store.getState().latestMoveOptions).toBe(moveOptions);
        });
    });

    describe("markPendingMoveAck", () => {
        it.each([true, false])(
            "should set isPendingMoveAck to true",
            (initial) => {
                store.setState({ isPendingMoveAck: initial });
                store.getState().markPendingMoveAck();
                expect(store.getState().isPendingMoveAck).toBe(true);
            },
        );
    });
});
