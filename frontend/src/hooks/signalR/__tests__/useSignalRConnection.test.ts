import { act, renderHook } from "@testing-library/react";
import * as signalR from "@microsoft/signalr";
import { mock, MockProxy } from "vitest-mock-extended";

import useSignalRConnection from "../useSignalRConnection";
import useSignalRStore, {
    initialSignalRStoreState,
} from "@/stores/signalRStore";
import { mockHubBuilder } from "@/lib/testUtils/mocks/mockSignalR";

vi.mock("@microsoft/signalr");

describe("useSignalRConnection", () => {
    let hubBuilderMethodMocks: MockProxy<signalR.HubConnectionBuilder>;

    beforeEach(() => {
        useSignalRStore.setState(initialSignalRStoreState);
        hubBuilderMethodMocks = mockHubBuilder();
    });

    function addHubConnection(
        hubUrl: string,
        mockConnection: signalR.HubConnection,
    ) {
        hubBuilderMethodMocks.build.mockReturnValue(mockConnection);
        const { getOrJoinHub } = renderHook(() => useSignalRStore()).result
            .current;
        act(() => getOrJoinHub(hubUrl));
    }

    it("should get or join hub and start the connection", async () => {
        const mockConnection = mock<signalR.HubConnection>({
            state: signalR.HubConnectionState.Disconnected,
            start: vi.fn().mockResolvedValue(undefined),
        });
        const hubUrl = "https://test.com/hub";
        addHubConnection(hubUrl, mockConnection);

        const { result } = renderHook(() => useSignalRConnection(hubUrl));

        expect(result.current).toBe(mockConnection);
        expect(mockConnection.start).toHaveBeenCalled();
    });

    it("should not start if already connected", async () => {
        const mockConnection = mock<signalR.HubConnection>({
            state: signalR.HubConnectionState.Connected,
        });
        const hubUrl = "https://test.com/hub";
        addHubConnection(hubUrl, mockConnection);

        const { result } = renderHook(() => useSignalRConnection(hubUrl));

        expect(result.current).toBe(mockConnection);
        expect(mockConnection.start).not.toHaveBeenCalled();
    });
});
