import { StoreApi } from "zustand";
import createLiveChessStore, {
    LiveChessStore,
    LiveChessStoreProps,
} from "../liveChessStore";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { createFakePosition } from "@/lib/testUtils/fakers/positionFaker";

describe("HistorySlice", () => {
    let store: StoreApi<LiveChessStore>;
    let initialProps: LiveChessStoreProps;

    beforeEach(() => {
        initialProps = createFakeLiveChessStoreProps();
        store = createLiveChessStore(initialProps);
    });

    describe("teleportToMove", () => {
        it("should return undefined if out of bounds", () => {
            expect(store.getState().teleportToMove(-1)).toBeUndefined();
            expect(
                store
                    .getState()
                    .teleportToMove(initialProps.positionHistory.length),
            ).toBeUndefined();
        });

        it("should update viewingMoveNumber and return correct BoardState", () => {
            const result = store.getState().teleportToMove(0);

            expect(store.getState().viewingMoveNumber).toBe(0);
            expect(result?.state.pieces).toBe(
                store.getState().positionHistory[0].pieces,
            );
            expect(result?.state.moveOptions.legalMoves.size).toBe(0);
            expect(result?.isOneStepForward).toBe(false);
        });

        it("should set moveThatProducedPosition to position.move for the viewed index", () => {
            const { positionHistory } = store.getState();

            const result = store.getState().teleportToMove(0);

            expect(result?.state.moveThatProducedPosition).toBe(
                positionHistory[0].move,
            );
        });

        it("should set moveFromPreviousViewedPosition = position.move when moving one step forward", () => {
            store.setState({ viewingMoveNumber: 0 });

            const { positionHistory } = store.getState();
            const result = store.getState().teleportToMove(1);

            expect(result?.state.moveFromPreviousViewedPosition).toBe(
                positionHistory[1].move,
            );
        });

        it("should set moveFromPreviousViewedPosition = next position's move when moving backward", () => {
            const { positionHistory } = store.getState();

            store.setState({ viewingMoveNumber: 1 });

            const result = store.getState().teleportToMove(0);

            expect(result?.state.moveFromPreviousViewedPosition).toBe(
                positionHistory[1].move,
            );
        });

        it("should return latest legal moves when getting latest position", () => {
            const lastIndex = initialProps.positionHistory.length - 1;
            const result = store.getState().teleportToMove(lastIndex);

            expect(result?.state.moveOptions).toEqual(
                initialProps.latestMoveOptions,
            );
            expect(result?.isOneStepForward).toBe(false);
        });

        it("should return isOneStepForward = true when moving exactly one step forward", () => {
            store.setState({ viewingMoveNumber: 0 });

            const result = store.getState().teleportToMove(1);

            expect(result?.isOneStepForward).toBe(true);
        });

        it("should return isOneStepForward = false when not exactly one step forward", () => {
            store.setState({ viewingMoveNumber: 0 });

            store.getState().teleportToMove(1);
            const result = store.getState().teleportToMove(0);

            expect(result?.isOneStepForward).toBe(false);
        });
    });

    describe("shiftMoveViewBy", () => {
        it("should shift the view by amount and return correct BoardState", () => {
            const initial = store.getState().viewingMoveNumber;
            const shifted = store.getState().shiftMoveViewBy(-1);
            expect(store.getState().viewingMoveNumber).toBe(initial - 1);
            expect(shifted?.state.pieces).toBe(
                store.getState().positionHistory[initial - 1].pieces,
            );
        });

        it("should return isOneStepForward = true when shifting +1", () => {
            store.setState({ viewingMoveNumber: 0 });

            const result = store.getState().shiftMoveViewBy(1);

            expect(result?.isOneStepForward).toBe(true);
        });

        it("should return isOneStepForward = false when shifting more than 1", () => {
            const extraPositions = [
                ...store.getState().positionHistory,
                createFakePosition({ san: "e5" }),
            ];
            store.setState({
                positionHistory: extraPositions,
                viewingMoveNumber: 0,
            });

            const result = store.getState().shiftMoveViewBy(2);

            expect(result?.isOneStepForward).toBe(false);
        });
    });

    describe("teleportToLastMove", () => {
        it("should teleport to last move and return BoardState", () => {
            const lastIndex = store.getState().positionHistory.length - 1;
            const state = store.getState().teleportToLastMove();
            expect(store.getState().viewingMoveNumber).toBe(lastIndex);
            expect(state?.state.pieces).toBe(
                store.getState().positionHistory[lastIndex].pieces,
            );
        });

        it("should throw error when positionHistory is empty", () => {
            store.setState({ positionHistory: [] });
            expect(() => store.getState().teleportToLastMove()).toThrow();
        });
    });

    describe("receivePosition", () => {
        it("should add the position to history", () => {
            const position = createFakePosition();
            const { positionHistory: previousHistory, receivePosition } =
                store.getState();

            receivePosition(position);

            const expectedHistory = [...previousHistory, position];
            expect(store.getState().positionHistory).toEqual(expectedHistory);
        });

        it("should increment viewingMoveNumber when currently viewing the latest move", () => {
            const position = createFakePosition();
            const { positionHistory } = store.getState();

            store.setState({
                viewingMoveNumber: positionHistory.length - 1,
            });

            store.getState().receivePosition(position);

            expect(store.getState().viewingMoveNumber).toBe(
                positionHistory.length,
            );
        });

        it("should not increment viewingMoveNumber when not viewing the latest move", () => {
            const position = createFakePosition();

            store.setState({ viewingMoveNumber: 0 });

            store.getState().receivePosition(position);

            expect(store.getState().viewingMoveNumber).toBe(0);
        });
    });
});
