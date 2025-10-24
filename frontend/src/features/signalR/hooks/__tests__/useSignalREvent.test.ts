import {
    addMockHubConnection,
    mockHubBuilder,
    mockHubConnection,
} from "@/lib/testUtils/mocks/mockSignalR";
import useSignalRStore from "@/features/signalR/stores/signalRStore";
import { renderHook } from "@testing-library/react";
import useSignalREvent from "../useSignalREvent";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";

describe("useSignalREvent", () => {
    const hubUrl = "https://test.com/hub";
    const eventName = "messageReceived";

    beforeEach(() => {
        useSignalRStore.setState(useSignalRStore.getInitialState());
    });

    it("should register event handler with connection.on", async () => {
        const { mockConnection } = mockHubConnection();
        const mockHandler = vi.fn();

        await addMockHubConnection(hubUrl, mockConnection);

        renderHook(() => useSignalREvent(hubUrl, eventName, mockHandler));
        await flushMicrotasks();

        expect(mockConnection.on).toHaveBeenCalledWith(eventName, mockHandler);
    });

    it("should not throw if connection is null", async () => {
        const mockHandler = vi.fn();
        mockHubBuilder();

        expect(() => {
            renderHook(() => useSignalREvent(hubUrl, eventName, mockHandler));
        }).not.toThrow();
        await flushMicrotasks();
    });

    it("should re-register handler if onEvent changes", async () => {
        const { mockConnection } = mockHubConnection();
        const handler1 = vi.fn();
        const handler2 = vi.fn();

        await addMockHubConnection(hubUrl, mockConnection);

        const { rerender } = renderHook(
            ({ handler }) => useSignalREvent(hubUrl, eventName, handler),
            { initialProps: { handler: handler1 } },
        );
        await flushMicrotasks();

        expect(mockConnection.on).toHaveBeenCalledWith(eventName, handler1);

        rerender({ handler: handler2 });

        expect(mockConnection.on).toHaveBeenCalledWith(eventName, handler2);
    });
});
