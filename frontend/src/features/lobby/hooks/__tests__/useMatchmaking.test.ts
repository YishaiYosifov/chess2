import { TimeControlSettings } from "@/lib/apiClient";
import { act, renderHook } from "@testing-library/react";
import useMatchmaking from "../useMatchmaking";
import {
    LobbyClientEvents,
    useLobbyEmitter,
    useLobbyEvent,
} from "@/features/signalR/hooks/useSignalRHubs";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";

vi.mock("@/features/signalR/hooks/useSignalRHubs");

describe("useMatchmaking", () => {
    const mockTimeControl: TimeControlSettings = {
        baseSeconds: 300,
        incrementSeconds: 5,
    };
    const lobbyHandlers: EventHandlers<LobbyClientEvents> = {};

    const sendLobbyEvent = vi.fn();
    const useLobbyEventMock = vi.mocked(useLobbyEvent);

    beforeEach(() => {
        vi.mocked(useLobbyEmitter).mockReturnValue(sendLobbyEvent);
        useLobbyEventMock.mockImplementation((event, handler) => {
            lobbyHandlers[event] = handler;
        });
    });

    it("should create a rated seek", async () => {
        const { result } = renderHook(() => useMatchmaking());

        await act(() => result.current.createSeek(true, mockTimeControl));

        expect(result.current.isSeeking).toBe(true);
        expect(sendLobbyEvent).toHaveBeenCalledWith(
            "SeekRatedAsync",
            mockTimeControl,
        );
    });

    it("should create a casual seek", async () => {
        const { result } = renderHook(() => useMatchmaking());

        await act(() => result.current.createSeek(false, mockTimeControl));

        expect(result.current.isSeeking).toBe(true);
        expect(sendLobbyEvent).toHaveBeenCalledWith(
            "SeekCasualAsync",
            mockTimeControl,
        );
    });

    it("should cancel a seek", async () => {
        const { result } = renderHook(() => useMatchmaking());

        await act(() => result.current.createSeek(true, mockTimeControl));

        await act(() => result.current.cancelSeek());

        expect(result.current.isSeeking).toBe(false);
        expect(sendLobbyEvent).toHaveBeenCalledWith("CancelSeekAsync");
    });

    it("should toggle seek on when not seeking", async () => {
        const { result } = renderHook(() => useMatchmaking());

        await act(() => result.current.toggleSeek(true, mockTimeControl));

        expect(result.current.isSeeking).toBe(true);
        expect(sendLobbyEvent).toHaveBeenCalledWith(
            "SeekRatedAsync",
            mockTimeControl,
        );
    });

    it("should toggle seek off when already seeking", async () => {
        const { result } = renderHook(() => useMatchmaking());

        await act(() => result.current.toggleSeek(true, mockTimeControl));

        await act(() => result.current.toggleSeek(true, mockTimeControl));

        expect(result.current.isSeeking).toBe(false);
        expect(sendLobbyEvent).toHaveBeenCalledWith("CancelSeekAsync");
    });

    it("should navigate to game on MatchFoundAsync event", () => {
        const { push } = mockRouter();
        renderHook(() => useMatchmaking());

        act(() => lobbyHandlers["MatchFoundAsync"]?.("abc123"));

        expect(push).toHaveBeenCalledWith("/game/abc123");
    });

    it("should stop seeking on MatchFailedAsync event", async () => {
        const { result } = renderHook(() => useMatchmaking());

        await act(() => result.current.createSeek(true, mockTimeControl));

        act(() => lobbyHandlers["MatchFailedAsync"]?.());

        expect(result.current.isSeeking).toBe(false);
    });
});
