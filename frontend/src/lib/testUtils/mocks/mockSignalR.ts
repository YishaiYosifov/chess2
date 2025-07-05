import * as signalR from "@microsoft/signalr";
import { mock, MockProxy } from "vitest-mock-extended";
import { Mock } from "vitest";
import { act, renderHook } from "@testing-library/react";
import useSignalRStore from "@/features/signalR/stores/signalRStore";

export function mockHubBuilder() {
    const mockHubBuilder = mock<signalR.HubConnectionBuilder>();
    const defaultHubConnection = mockHubConnection();

    mockHubBuilder.withUrl.mockReturnThis();
    mockHubBuilder.withAutomaticReconnect.mockReturnThis();
    mockHubBuilder.configureLogging.mockReturnThis();
    mockHubBuilder.build.mockReturnValue(defaultHubConnection);

    (signalR.HubConnectionBuilder as Mock).mockReturnValue(mockHubBuilder);

    return mockHubBuilder;
}

export function mockHubConnection(
    state: signalR.HubConnectionState = signalR.HubConnectionState.Disconnected,
) {
    const mockHubConnection = mock<signalR.HubConnection>({
        start: vi.fn().mockResolvedValue(undefined),
        stop: vi.fn().mockResolvedValue(undefined),
        state,
    });

    return mockHubConnection;
}

export function addMockHubConnection(
    hubBuilderMethodMocks: MockProxy<signalR.HubConnectionBuilder>,
    hubUrl: string,
    mockConnection: signalR.HubConnection,
) {
    hubBuilderMethodMocks.build.mockReturnValue(mockConnection);
    const { joinHub: getOrJoinHub } = renderHook(() => useSignalRStore()).result
        .current;
    act(() => getOrJoinHub(hubUrl));
}
