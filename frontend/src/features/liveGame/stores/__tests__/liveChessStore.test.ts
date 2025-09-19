import { StoreApi } from "zustand";
import createLiveChessStore, {
    LiveChessStore,
    LiveChessStoreProps,
} from "../liveChessStore";
import { Clocks, GameColor, GameResult, GameResultData } from "@/lib/apiClient";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { createFakePosition } from "@/lib/testUtils/fakers/positionFaker";
import { createFakeClock } from "@/lib/testUtils/fakers/clockFaker";
import { ProcessedMoveOptions } from "@/features/chessboard/lib/types";
import { LegalMoveMap } from "@/features/chessboard/lib/types";
import { createFakeLegalMoveMap } from "@/lib/testUtils/fakers/chessboardFakers";
import { createMoveOptions } from "@/features/chessboard/lib/moveOptions";

describe("LiveChessStore", () => {
    let store: StoreApi<LiveChessStore>;
    let initialProps: LiveChessStoreProps;

    beforeEach(() => {
        initialProps = createFakeLiveChessStoreProps();
        store = createLiveChessStore(initialProps);
    });

    it("should initialize with correct viewingMoveNumber", () => {
        expect(store.getState().viewingMoveNumber).toBe(
            initialProps.positionHistory.length - 1,
        );
    });

    describe("receiveMove", () => {
        it("should increment viewingMoveNumber if viewing at latest move and push position", () => {
            const newPosition = createFakePosition();
            const newClocks: Clocks = { whiteClock: 500, blackClock: 600 };
            const newSideToMove = GameColor.BLACK;

            store.getState().receiveMove(newPosition, newClocks, newSideToMove);
            const state = store.getState();

            expect(state.positionHistory).toContain(newPosition);
            expect(state.clocks).toBe(newClocks);
            expect(state.sideToMove).toBe(newSideToMove);
            expect(state.viewingMoveNumber).toBe(
                initialProps.positionHistory.length,
            );
        });

        it("should not increment viewingMoveNumber if not at latest move", () => {
            store.setState({ viewingMoveNumber: 0 });
            const oldView = store.getState().viewingMoveNumber;

            store
                .getState()
                .receiveMove(
                    createFakePosition(),
                    createFakeClock(),
                    GameColor.BLACK,
                );

            expect(store.getState().viewingMoveNumber).toBe(oldView);
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

    describe("endGame", () => {
        it("should update player ratings and set resultData and clear latestMoveOptions", () => {
            const resultData: GameResultData = {
                result: GameResult.BLACK_WIN,
                resultDescription: "test description",
                whiteRatingChange: -10,
                blackRatingChange: 10,
            };
            store.getState().endGame(resultData);
            const state = store.getState();

            expect(state.whitePlayer.rating).toBe(
                initialProps.whitePlayer.rating! +
                    resultData.whiteRatingChange!,
            );
            expect(state.blackPlayer.rating).toBe(
                initialProps.blackPlayer.rating! +
                    resultData.blackRatingChange!,
            );
            expect(state.resultData).toBe(resultData);
            expect(state.latestMoveOptions).toEqual<ProcessedMoveOptions>({
                legalMoves: new Map(),
                hasForcedMoves: false,
            });
        });
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
            expect(result?.state.casuedByMove).toBe(
                store.getState().positionHistory[0].move,
            );
            expect(result?.isOneStepForward).toBe(false);
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

    describe("resetState", () => {
        it("should reset the state to props", () => {
            const newChessboardState = createFakeLiveChessStoreProps();

            store.getState().resetState(newChessboardState);

            const state = store.getState();
            expect(state).toEqual({
                ...store.getInitialState(),
                ...newChessboardState,
            });
        });
    });
});
