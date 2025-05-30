import {
    addMockHubConnection,
    mockHubBuilder,
    mockHubConnection,
} from "@/lib/testUtils/mocks/mockSignalR";
import useSignalRStore, {
    initialSignalRStoreState,
} from "@/stores/signalRStore";
import * as signalR from "@microsoft/signalr";
import { renderHook } from "@testing-library/react";
import { MockProxy } from "vitest-mock-extended";
import useSignalREvent from "../useSignalREvent";

vi.mock("@microsoft/signalr");

describe("useSignalREvent", () => {
    let hubBuilderInstanceMock: MockProxy<signalR.HubConnectionBuilder>;
    const hubUrl = "https://test.com/hub";
    const eventName = "messageReceived";

    beforeEach(() => {
        useSignalRStore.setState(initialSignalRStoreState);
        hubBuilderInstanceMock = mockHubBuilder();
    });

    it("should register event handler with connection.on", () => {
        const mockConnection = mockHubConnection();
        const mockHandler = vi.fn();

        // Inject connection into store
        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        renderHook(() => useSignalREvent(hubUrl, eventName, mockHandler));

        expect(mockConnection.on).toHaveBeenCalledWith(eventName, mockHandler);
    });

    it("should not throw if connection is null", () => {
        const mockHandler = vi.fn();

        expect(() => {
            renderHook(() => useSignalREvent(hubUrl, eventName, mockHandler));
        }).not.toThrow();
    });

    it("should re-register handler if onEvent changes", () => {
        const mockConnection = mockHubConnection();
        const handler1 = vi.fn();
        const handler2 = vi.fn();

        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        const { rerender } = renderHook(
            ({ handler }) => useSignalREvent(hubUrl, eventName, handler),
            { initialProps: { handler: handler1 } },
        );

        expect(mockConnection.on).toHaveBeenCalledWith(eventName, handler1);

        rerender({ handler: handler2 });

        expect(mockConnection.on).toHaveBeenCalledWith(eventName, handler2);
    });
});
