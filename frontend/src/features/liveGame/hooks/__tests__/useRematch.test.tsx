import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import createLiveChessStore, {
    LiveChessStore,
} from "../../stores/liveChessStore";
import useRematch from "../useRematch";
import { act, renderHook } from "@testing-library/react";
import LiveChessStoreContext from "../../contexts/liveChessContext";
import { StoreApi } from "zustand";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import { GameClientEvents, useGameEmitter, useGameEvent } from "../useGameHub";
import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";
import constants from "@/lib/constants";

vi.mock("@/features/liveGame/hooks/useGameHub");

describe("useRematch", () => {
    let liveChessStore: StoreApi<LiveChessStore>;

    const sendGameEventMock = vi.fn();

    const gameToken = "game-token-123";
    const gameHandlers: EventHandlers<GameClientEvents> = {};

    beforeEach(() => {
        vi.clearAllMocks();
        vi.useFakeTimers();

        liveChessStore = createLiveChessStore(
            createFakeLiveChessStoreProps({ gameToken }),
        );

        vi.mocked(useGameEmitter).mockReturnValue(sendGameEventMock);
        vi.mocked(useGameEvent).mockImplementation((token, event, handler) => {
            if (token === gameToken) gameHandlers[event] = handler;
        });
    });

    function renderRematchHook() {
        return renderHook(() => useRematch(), {
            wrapper: ({ children }) => (
                <LiveChessStoreContext.Provider value={liveChessStore}>
                    {children}
                </LiveChessStoreContext.Provider>
            ),
        });
    }

    it("should request a rematch", async () => {
        const { result } = renderRematchHook();

        await act(() => result.current.requestRematch());

        expect(sendGameEventMock).toHaveBeenCalledWith(
            "RequestRematchAsync",
            gameToken,
        );
        expect(liveChessStore.getState().isRequestingRematch).toBe(true);
    });

    it("should cancel a rematch", async () => {
        const { result } = renderRematchHook();

        await act(() => result.current.cancelRematch());

        expect(sendGameEventMock).toHaveBeenCalledWith(
            "CancelRematchAsync",
            gameToken,
        );
        expect(liveChessStore.getState().isRequestingRematch).toBe(false);
    });

    it("should toggle rematch on when not requesting", async () => {
        const { result } = renderRematchHook();

        await act(() => result.current.toggleRematch());

        expect(sendGameEventMock).toHaveBeenCalledWith(
            "RequestRematchAsync",
            gameToken,
        );
        expect(liveChessStore.getState().isRequestingRematch).toBe(true);
    });

    it("should toggle rematch off when already requesting", async () => {
        const { result } = renderRematchHook();

        await act(() => result.current.requestRematch());
        expect(liveChessStore.getState().isRequestingRematch).toBe(true);

        await act(() => result.current.toggleRematch());

        expect(sendGameEventMock).toHaveBeenCalledWith(
            "CancelRematchAsync",
            gameToken,
        );
        expect(liveChessStore.getState().isRequestingRematch).toBe(false);
    });

    it("should set isRematchRequested to true when RematchRequestedAsync fires", async () => {
        const { result } = renderRematchHook();

        expect(result.current.isRematchRequested).toBe(false);

        await act(() => gameHandlers["RematchRequestedAsync"]?.());

        expect(result.current.isRematchRequested).toBe(true);
    });

    it("should set isRematchRequested to false when RematchCancelledAsync fires", async () => {
        const { result } = renderRematchHook();

        // set to true first
        act(() => gameHandlers["RematchRequestedAsync"]?.());
        expect(result.current.isRematchRequested).toBe(true);

        await act(() => gameHandlers["RematchCancelledAsync"]?.());
        expect(result.current.isRematchRequested).toBe(false);
    });

    it("should navigate when RematchAccepted fires", async () => {
        const routerMock = mockRouter();
        renderRematchHook();
        const newGameToken = "new-game-token-999";

        await act(() => gameHandlers["RematchAccepted"]?.(newGameToken));

        expect(routerMock.push).toHaveBeenCalledWith(
            `${constants.PATHS.GAME}/${newGameToken}`,
        );
    });
});
