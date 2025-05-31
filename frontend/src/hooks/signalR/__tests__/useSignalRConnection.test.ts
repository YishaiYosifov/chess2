import { renderHook } from "@testing-library/react";
import { MockProxy } from "vitest-mock-extended";
import * as signalR from "@microsoft/signalr";

import useSignalRConnection from "../useSignalRConnection";
import useSignalRStore, {
    initialSignalRStoreState,
} from "@/stores/signalRStore";
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

    it("should get or join hub and start the connection", async () => {
        const mockConnection = mockHubConnection();
        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        const { result } = renderHook(() => useSignalRConnection(hubUrl));

        expect(result.current).toBe(mockConnection);
        expect(mockConnection.start).toHaveBeenCalled();
    });

    it("should not start if already connected", async () => {
        const mockConnection = mockHubConnection(
            signalR.HubConnectionState.Connected,
        );
        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        const { result } = renderHook(() => useSignalRConnection(hubUrl));

        expect(result.current).toBe(mockConnection);
        expect(mockConnection.start).not.toHaveBeenCalled();
    });

    it("should handle connection errors", async () => {
        const mockConnection = mockHubConnection(
            signalR.HubConnectionState.Disconnected,
        );
        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        renderHook(() => useSignalRConnection(hubUrl));

        expect(mockConnection.on).toHaveBeenCalledExactlyOnceWith(
            "ReceiveErrorAsync",
            console.error,
        );
    });
});
