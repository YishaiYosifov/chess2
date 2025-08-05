import {
    HubConnection,
    HubConnectionBuilder,
    HubConnectionState,
} from "@microsoft/signalr";

import { mock, MockProxy } from "vitest-mock-extended";
import { act, renderHook } from "@testing-library/react";
import useSignalRStore from "@/features/signalR/stores/signalRStore";

export function mockHubBuilder() {
    const mockHubBuilder = mock<HubConnectionBuilder>();
    const { mockConnection } = mockHubConnection();

    mockHubBuilder.withUrl.mockReturnThis();
    mockHubBuilder.withAutomaticReconnect.mockReturnThis();
    mockHubBuilder.configureLogging.mockReturnThis();
    mockHubBuilder.build.mockReturnValue(mockConnection);

    vi.mocked(HubConnectionBuilder).mockReturnValue(mockHubBuilder);

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
        start: vi.fn().mockResolvedValue(undefined),
        stop: vi.fn().mockResolvedValue(undefined),
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

export function addMockHubConnection(
    hubBuilderMethodMocks: MockProxy<HubConnectionBuilder>,
    hubUrl: string,
    mockConnection: HubConnection,
) {
    hubBuilderMethodMocks.build.mockReturnValue(mockConnection);
    const { joinHub: getOrJoinHub } = renderHook(() => useSignalRStore()).result
        .current;
    act(() => getOrJoinHub(hubUrl));
}
