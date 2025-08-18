import { PoolKey, PoolType, TimeControlSettings } from "@/lib/apiClient";
import { act, renderHook } from "@testing-library/react";
import useMatchmaking from "../useMatchmaking";
import {
    LobbyClientEvents,
    useLobbyEmitter,
    useLobbyEvent,
} from "@/features/signalR/hooks/useSignalRHubs";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import constants from "@/lib/constants";

vi.mock("@/features/signalR/hooks/useSignalRHubs");

describe("useMatchmaking", () => {
    const timeControl: TimeControlSettings = {
        baseSeconds: 300,
        incrementSeconds: 5,
    };
    const poolRated: PoolKey = {
        poolType: PoolType.RATED,
        timeControl: timeControl,
    };
    const poolCasual: PoolKey = {
        poolType: PoolType.CASUAL,
        timeControl: timeControl,
    };

    const lobbyHandlers: EventHandlers<LobbyClientEvents> = {};
    const sendLobbyEvent = vi.fn();

    beforeEach(() => {
        vi.useFakeTimers();
        vi.mocked(useLobbyEmitter).mockReturnValue(sendLobbyEvent);
        vi.mocked(useLobbyEvent).mockImplementation((event, handler) => {
            lobbyHandlers[event] = handler;
        });
    });

    it("should create a rated seek", async () => {
        const { result } = renderHook(() => useMatchmaking(poolRated));

        await act(() => result.current.createSeek());

        expect(result.current.isSeeking).toBe(true);
        expect(sendLobbyEvent).toHaveBeenCalledWith(
            "SeekRatedAsync",
            timeControl,
        );
    });

    it("should create a casual seek", async () => {
        const { result } = renderHook(() => useMatchmaking(poolCasual));

        await act(() => result.current.createSeek());

        expect(result.current.isSeeking).toBe(true);
        expect(sendLobbyEvent).toHaveBeenCalledWith(
            "SeekCasualAsync",
            timeControl,
        );
    });

    it("should cancel a seek", async () => {
        const { result } = renderHook(() => useMatchmaking(poolRated));

        await act(() => result.current.createSeek());
        await act(() => result.current.cancelSeek());

        expect(result.current.isSeeking).toBe(false);
        expect(sendLobbyEvent).toHaveBeenCalledWith(
            "CancelSeekAsync",
            poolRated,
        );
    });

    it("should toggle seek on when not seeking", async () => {
        const { result } = renderHook(() => useMatchmaking(poolRated));

        await act(() => result.current.toggleSeek());

        expect(result.current.isSeeking).toBe(true);
        expect(sendLobbyEvent).toHaveBeenCalledWith(
            "SeekRatedAsync",
            timeControl,
        );
    });

    it("should toggle seek off when already seeking", async () => {
        const { result } = renderHook(() => useMatchmaking(poolRated));

        await act(() => result.current.toggleSeek());
        await act(() => result.current.toggleSeek());

        expect(result.current.isSeeking).toBe(false);
        expect(sendLobbyEvent).toHaveBeenCalledWith(
            "CancelSeekAsync",
            poolRated,
        );
    });

    it("should stop seeking when SeekFailedAsync fires for this pool", async () => {
        const { result } = renderHook(() => useMatchmaking(poolRated));

        await act(() => result.current.createSeek());
        expect(result.current.isSeeking).toBe(true);

        act(() => lobbyHandlers["SeekFailedAsync"]?.(poolRated));

        expect(result.current.isSeeking).toBe(false);
    });

    it("should ignore SeekFailedAsync for a different pool", async () => {
        const { result } = renderHook(() => useMatchmaking(poolRated));
        const poolOther: PoolKey = {
            poolType: PoolType.CASUAL,
            timeControl: timeControl,
        };

        await act(() => result.current.createSeek());
        expect(result.current.isSeeking).toBe(true);

        act(() => lobbyHandlers["SeekFailedAsync"]?.(poolOther));

        expect(result.current.isSeeking).toBe(true);
    });

    it("should repeatedly send seek requests at interval", async () => {
        const { result } = renderHook(() => useMatchmaking(poolRated));

        await act(() => result.current.createSeek());

        expect(sendLobbyEvent).toHaveBeenCalledTimes(1);
        expect(sendLobbyEvent).toHaveBeenCalledWith(
            "SeekRatedAsync",
            timeControl,
        );

        act(() => {
            vi.advanceTimersByTime(constants.SEEK_RESUBSCRIBE_INTERAVAL_MS);
        });

        expect(sendLobbyEvent).toHaveBeenCalledTimes(2);

        act(() => {
            vi.advanceTimersByTime(2 * constants.SEEK_RESUBSCRIBE_INTERAVAL_MS);
        });

        expect(sendLobbyEvent).toHaveBeenCalledTimes(4);
    });

    it("should stop interval when cancelSeek is called", async () => {
        const { result } = renderHook(() => useMatchmaking(poolRated));

        await act(() => result.current.createSeek());

        act(() => {
            vi.advanceTimersByTime(constants.SEEK_RESUBSCRIBE_INTERAVAL_MS);
        });
        expect(sendLobbyEvent).toHaveBeenCalledTimes(2);

        await act(() => result.current.cancelSeek());
        expect(result.current.isSeeking).toBe(false);

        act(() => {
            vi.advanceTimersByTime(5 * constants.SEEK_RESUBSCRIBE_INTERAVAL_MS);
        });

        // 1 call for cancel, otherwise no new create seek calls
        expect(sendLobbyEvent).toHaveBeenCalledTimes(3);
    });
});
