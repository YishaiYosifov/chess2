import { HubConnectionState } from "@microsoft/signalr";
import { act, renderHook } from "@testing-library/react";
import { MockProxy } from "vitest-mock-extended";

import useSignalRConnection from "../useSignalRConnection";
import useSignalRStore, {
    initialSignalRStoreState,
} from "@/features/signalR/stores/signalRStore";
import {
    addMockHubConnection,
    mockHubBuilder,
    mockHubConnection,
} from "@/lib/testUtils/mocks/mockSignalR";

vi.mock("@microsoft/signalr");

describe("useSignalRConnection", () => {
    let hubBuilderInstanceMock: MockProxy<signalR.HubConnectionBuilder>;
    const hubUrl = "https://test.com/hub";

    beforeEach(() => {
        useSignalRStore.setState(initialSignalRStoreState);
        hubBuilderInstanceMock = mockHubBuilder();
    });

    it("should join the hub and start connection if disconnected", async () => {
        const { mockConnection } = mockHubConnection(
            HubConnectionState.Disconnected,
        );
        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        const { result } = renderHook(() => useSignalRConnection(hubUrl));

        // await the connection.start promise resolution
        await act(() => Promise.resolve());

        expect(result.current.connection).toBe(mockConnection);
        expect(result.current.state).toBe(HubConnectionState.Connected);
        expect(mockConnection.start).toHaveBeenCalled();
    });

    it("should not start connection if already connected", () => {
        const { mockConnection } = mockHubConnection(
            HubConnectionState.Connected,
        );
        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        const { result } = renderHook(() => useSignalRConnection(hubUrl));

        expect(result.current.connection).toBe(mockConnection);
        expect(result.current.state).toBe(HubConnectionState.Disconnected);
        expect(mockConnection.start).not.toHaveBeenCalled();
    });

    it("should handle ReceiveErrorAsync event registration", async () => {
        const { mockConnection } = mockHubConnection(
            HubConnectionState.Disconnected,
        );
        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        renderHook(() => useSignalRConnection(hubUrl));
        await act(() => Promise.resolve());

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

        const { result } = renderHook(() => useSignalRConnection(hubUrl));

        await act(() => Promise.resolve());

        act(() => handlers.onReconnectedHandler?.());
        expect(result.current.state).toBe(HubConnectionState.Connected);

        act(() => handlers.onCloseHandler?.());
        expect(result.current.state).toBe(HubConnectionState.Disconnected);
    });

    it("should clean up event handlers on unmount", () => {
        const { mockConnection } = mockHubConnection(
            HubConnectionState.Disconnected,
        );
        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        const { unmount } = renderHook(() => useSignalRConnection(hubUrl));
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
