import { HubConnectionState } from "@microsoft/signalr";
import { renderHook } from "@testing-library/react";

import useSignalRConnection from "../useSignalRConnection";
import useSignalRStore from "@/features/signalR/stores/signalRStore";
import {
    addMockHubConnection,
    mockHubBuilder,
    mockHubConnection,
} from "@/lib/testUtils/mocks/mockSignalR";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";

describe("useSignalRConnection", () => {
    const hubUrl = "https://test.com/hub";

    beforeEach(() => {
        useSignalRStore.setState(useSignalRStore.getInitialState());
    });

    it("should join the hub on mount and return connection", async () => {
        const { mockConnection } = mockHubConnection(
            HubConnectionState.Disconnected,
        );
        mockHubBuilder(mockConnection);

        const { result } = renderHook(() => useSignalRConnection(hubUrl));

        const hub = useSignalRStore.getState().hubs.get(hubUrl);
        expect(hub?.connection).toBe(mockConnection);
        expect(hub?.referenceCount).toBe(1);
        expect(mockConnection.start).toHaveBeenCalled();
        expect(result.current).toBe(mockConnection);
    });

    it("should increment referenceCount when multiple hooks mount the same hub", async () => {
        const { mockConnection } = mockHubConnection();
        mockHubBuilder(mockConnection);

        // reference both hooks
        const hook1 = renderHook(() => useSignalRConnection(hubUrl));
        let hub = useSignalRStore.getState().hubs.get(hubUrl);
        expect(hub?.referenceCount).toBe(1);

        const hook2 = renderHook(() => useSignalRConnection(hubUrl));
        hub = useSignalRStore.getState().hubs.get(hubUrl);
        expect(hub?.referenceCount).toBe(2);

        expect(hook1.result.current).toBe(mockConnection);
        expect(hook2.result.current).toBe(mockConnection);

        await flushMicrotasks();

        // dereference both hooks
        hook2.unmount();
        hub = useSignalRStore.getState().hubs.get(hubUrl);
        expect(hub?.referenceCount).toBe(1);

        hook1.unmount();
        hub = useSignalRStore.getState().hubs.get(hubUrl);
        expect(hub).toBeUndefined();
    });

    it("should deregister and stop connection on unmount when last reference", async () => {
        const { mockConnection } = mockHubConnection(
            HubConnectionState.Connected,
        );
        mockHubBuilder(mockConnection);

        const { unmount } = renderHook(() => useSignalRConnection(hubUrl));

        unmount();

        const hub = useSignalRStore.getState().hubs.get(hubUrl);
        expect(hub).toBeUndefined();
        expect(mockConnection.stop).toHaveBeenCalled();
    });

    it("should return the same connection if already connected", async () => {
        const { mockConnection } = mockHubConnection(
            HubConnectionState.Connected,
        );
        await addMockHubConnection(hubUrl, mockConnection);
        mockConnection.start.mockReset();

        const { result } = renderHook(() => useSignalRConnection(hubUrl));

        const hub = useSignalRStore.getState().hubs.get(hubUrl);
        expect(hub?.connection).toBe(mockConnection);
        expect(mockConnection.start).not.toHaveBeenCalled();
        expect(result.current).toBe(mockConnection);
    });
});
