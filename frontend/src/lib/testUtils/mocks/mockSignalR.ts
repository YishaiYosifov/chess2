import {
    HubConnection,
    HubConnectionBuilder,
    HubConnectionState,
} from "@microsoft/signalr";

import { mock } from "vitest-mock-extended";
import { act, renderHook } from "@testing-library/react";
import useSignalRStore from "@/features/signalR/stores/signalRStore";
import flushMicrotasks from "../flushMicrotasks";

export function mockHubBuilder(connection?: HubConnection) {
    const mockHubBuilder = mock<HubConnectionBuilder>();
    connection ??= mockHubConnection().mockConnection;

    mockHubBuilder.withUrl.mockReturnThis();
    mockHubBuilder.withAutomaticReconnect.mockReturnThis();
    mockHubBuilder.configureLogging.mockReturnThis();
    mockHubBuilder.build.mockReturnValue(connection);

    vi.mocked(HubConnectionBuilder).mockImplementation(function () {
        return mockHubBuilder;
    });

    return mockHubBuilder;
}

export function mockHubConnection(
    state: HubConnectionState = HubConnectionState.Disconnected,
) {
    const handlers = {
        onCloseHandler: undefined as (() => void) | undefined,
        onReconnectedHandler: undefined as (() => void) | undefined,
    };

    const mockConnection = mock<HubConnection>({
        start: vi.fn(async () => {}),
        stop: vi.fn(async () => {}),
        state,
        onclose: vi.fn((cb: () => void) => {
            handlers.onCloseHandler = cb;
        }),
        onreconnected: vi.fn((cb: () => void) => {
            handlers.onReconnectedHandler = cb;
        }),
    });

    return { mockConnection, handlers };
}

export async function addMockHubConnection(
    hubUrl: string,
    mockConnection: HubConnection,
) {
    mockHubBuilder(mockConnection);
    const { joinHub } = renderHook(() => useSignalRStore()).result.current;
    act(() => joinHub(hubUrl));
    await flushMicrotasks();
}
