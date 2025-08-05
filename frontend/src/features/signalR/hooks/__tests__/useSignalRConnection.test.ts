import { HubConnectionState } from "@microsoft/signalr";
import { act, renderHook } from "@testing-library/react";
import { MockProxy } from "vitest-mock-extended";

import useSignalRConnection from "../useSignalRConnection";
import useSignalRStore from "@/features/signalR/stores/signalRStore";
import {
    addMockHubConnection,
    mockHubBuilder,
    mockHubConnection,
} from "@/lib/testUtils/mocks/mockSignalR";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";

describe("useSignalRConnection", () => {
    let hubBuilderInstanceMock: MockProxy<signalR.HubConnectionBuilder>;
    const hubUrl = "https://test.com/hub";

    beforeEach(() => {
        useSignalRStore.setState(useSignalRStore.getInitialState());
        hubBuilderInstanceMock = mockHubBuilder();
    });

    it("should join the hub and start connection if disconnected", async () => {
        const { mockConnection } = mockHubConnection(
            HubConnectionState.Disconnected,
        );
        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        const { result } = renderHook(() => useSignalRConnection(hubUrl));
        await flushMicrotasks();

        expect(result.current).toBe(mockConnection);
        expect(mockConnection.start).toHaveBeenCalled();
    });

    it("should not start connection if already connected", async () => {
        const { mockConnection } = mockHubConnection(
            HubConnectionState.Connected,
        );
        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        const { result } = renderHook(() => useSignalRConnection(hubUrl));
        await flushMicrotasks();

        expect(result.current).toBe(mockConnection);
        expect(mockConnection.start).not.toHaveBeenCalled();
    });

    it("should handle ReceiveErrorAsync event registration", async () => {
        const { mockConnection } = mockHubConnection(
            HubConnectionState.Disconnected,
        );
        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        renderHook(() => useSignalRConnection(hubUrl));
        await flushMicrotasks();

        expect(mockConnection.on).toHaveBeenCalledWith(
            "ReceiveErrorAsync",
            console.error,
        );
    });

    it("should handle reconnected and onclose events", async () => {
        const { mockConnection, handlers } = mockHubConnection(
            HubConnectionState.Disconnected,
        );

        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        renderHook(() => useSignalRConnection(hubUrl));
        await flushMicrotasks();

        act(() => handlers.onReconnectedHandler?.());
        expect(useSignalRStore.getState().hubStates.get(hubUrl)).toBe(
            HubConnectionState.Connected,
        );

        act(() => handlers.onCloseHandler?.());
        expect(useSignalRStore.getState().hubStates.get(hubUrl)).toBe(
            HubConnectionState.Disconnected,
        );
    });

    it("should clean up event handlers on unmount", async () => {
        const { mockConnection } = mockHubConnection(
            HubConnectionState.Disconnected,
        );
        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        const { unmount } = renderHook(() => useSignalRConnection(hubUrl));
        await flushMicrotasks();
        unmount();

        expect(mockConnection.off).toHaveBeenCalledWith(
            "ReceiveErrorAsync",
            console.error,
        );
        expect(mockConnection.off).toHaveBeenCalledWith(
            "close",
            expect.any(Function),
        );
        expect(mockConnection.off).toHaveBeenCalledWith(
            "reconnected",
            expect.any(Function),
        );
    });
});
