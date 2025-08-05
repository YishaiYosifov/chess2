import {
    addMockHubConnection,
    mockHubBuilder,
    mockHubConnection,
} from "@/lib/testUtils/mocks/mockSignalR";
import useSignalRStore from "@/features/signalR/stores/signalRStore";
import { act, renderHook } from "@testing-library/react";
import useSignalREmitter from "../useSignalREmitter";
import { MockProxy } from "vitest-mock-extended";
import { HubConnectionState } from "@microsoft/signalr";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";

describe("useSignalREvent", () => {
    let hubBuilderInstanceMock: MockProxy<signalR.HubConnectionBuilder>;
    const hubUrl = "https://test.com/hub";

    beforeEach(() => {
        useSignalRStore.setState(useSignalRStore.getInitialState());
        hubBuilderInstanceMock = mockHubBuilder();
    });

    it("should return a sendEvent function that calls invoke on the connection", async () => {
        const { mockConnection } = mockHubConnection(
            HubConnectionState.Connected,
        );
        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        const { result } = renderHook(() => useSignalREmitter(hubUrl));
        await flushMicrotasks();

        await act(async () => {
            result.current("myEvent", "testArg", 42);
        });

        expect(mockConnection.invoke).toHaveBeenCalledWith(
            "myEvent",
            "testArg",
            42,
        );
    });

    it("should not crash if connection is null", async () => {
        const { result } = renderHook(() => useSignalREmitter(hubUrl));

        await act(() => result.current("myEvent", "arg"));
    });

    it("should queue events if connection is not ready and flush them after connection is established", async () => {
        const { mockConnection, handlers } = mockHubConnection(
            HubConnectionState.Disconnected,
        );

        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);
        mockConnection.start.mockRejectedValue(
            new Error("intentional mock start failure"),
        );

        const { result } = renderHook(() => useSignalREmitter(hubUrl));

        await act(async () => {
            await result.current("BufferedEvent", "pendingArg", 999);
        });

        expect(mockConnection.invoke).not.toHaveBeenCalled();

        act(() => handlers.onReconnectedHandler?.());

        await act(async () => {
            await Promise.resolve();
        });

        expect(mockConnection.invoke).toHaveBeenCalledWith(
            "BufferedEvent",
            "pendingArg",
            999,
        );
    });

    it("should flush multiple pending events when connection becomes ready", async () => {
        const { mockConnection, handlers } = mockHubConnection(
            HubConnectionState.Disconnected,
        );
        mockConnection.start.mockRejectedValue(
            new Error("intentional mock start failure"),
        );

        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        const { result } = renderHook(() => useSignalREmitter(hubUrl));

        await act(async () => {
            await result.current("Event1", "arg1");
            await result.current("Event2", "arg2");
        });

        expect(mockConnection.invoke).not.toHaveBeenCalled();

        act(() => handlers.onReconnectedHandler?.());

        await act(async () => {
            await Promise.resolve();
        });

        expect(mockConnection.invoke).toHaveBeenCalledWith("Event1", "arg1");
        expect(mockConnection.invoke).toHaveBeenCalledWith("Event2", "arg2");
        expect(mockConnection.invoke).toHaveBeenCalledTimes(2);
    });
});
