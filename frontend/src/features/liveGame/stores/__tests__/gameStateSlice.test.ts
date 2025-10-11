import { StoreApi } from "zustand";
import createLiveChessStore, {
    LiveChessStore,
    LiveChessStoreProps,
} from "../liveChessStore";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { ProcessedMoveOptions } from "@/features/chessboard/lib/types";
import { DrawState, GameResult, GameResultData } from "@/lib/apiClient";
import { createFakeDrawState } from "@/lib/testUtils/fakers/drawStateFaker";

describe("GameStateSlice", () => {
    let store: StoreApi<LiveChessStore>;
    let initialProps: LiveChessStoreProps;

    beforeEach(() => {
        initialProps = createFakeLiveChessStoreProps();
        store = createLiveChessStore(initialProps);
    });

    describe("decrementDrawCooldown", () => {
        it("should decrement draw cooldown for both players", () => {
            const drawState: DrawState = {
                whiteCooldown: 10,
                blackCooldown: 10,
                activeRequester: null,
            };
            store.setState({ drawState });

            store.getState().decrementDrawCooldown();

            const expectedDrawState: DrawState = {
                whiteCooldown: 9,
                blackCooldown: 9,
                activeRequester: null,
            };
            expect(store.getState().drawState).toEqual(expectedDrawState);
        });

        it("should not go bellow 0", () => {
            const drawState: DrawState = {
                whiteCooldown: 0,
                blackCooldown: 10,
                activeRequester: null,
            };
            store.setState({ drawState });

            store.getState().decrementDrawCooldown();

            const expectedDrawState: DrawState = {
                whiteCooldown: 0,
                blackCooldown: 9,
                activeRequester: null,
            };
            expect(store.getState().drawState).toEqual(expectedDrawState);
        });
    });

    describe("drawStateChange", () => {
        it("should add the position to history", () => {
            const drawState = createFakeDrawState();
            const { drawStateChange } = store.getState();

            drawStateChange(drawState);

            expect(store.getState().drawState).toEqual(drawState);
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
