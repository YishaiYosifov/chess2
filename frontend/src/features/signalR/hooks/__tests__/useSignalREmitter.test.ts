import {
    addMockHubConnection,
    mockHubBuilder,
    mockHubConnection,
} from "@/lib/testUtils/mocks/mockSignalR";
import useSignalRStore, {
    initialSignalRStoreState,
} from "@/features/signalR/stores/signalRStore";
import { act, renderHook } from "@testing-library/react";
import useSignalREmitter from "../../../../hooks/signalR/useSignalREmitter";
import { MockProxy } from "vitest-mock-extended";

vi.mock("@microsoft/signalr");

describe("useSignalREvent", () => {
    let hubBuilderInstanceMock: MockProxy<signalR.HubConnectionBuilder>;
    const hubUrl = "https://test.com/hub";

    beforeEach(() => {
        useSignalRStore.setState(initialSignalRStoreState);
        hubBuilderInstanceMock = mockHubBuilder();
    });

    it("should return a sendEvent function that calls invoke on the connection", async () => {
        const mockConnection = mockHubConnection();
        addMockHubConnection(hubBuilderInstanceMock, hubUrl, mockConnection);

        const { result } = renderHook(() => useSignalREmitter(hubUrl));

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

        await act(async () => {
            // This should not throw or call invoke
            result.current("myEvent", "arg");
        });
    });
});
