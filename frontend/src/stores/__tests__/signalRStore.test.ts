import { renderHook } from "@testing-library/react";
import * as signalR from "@microsoft/signalr";
import { mock, MockProxy } from "vitest-mock-extended";
import { Mock } from "vitest";
import { act } from "react";

import useSignalRStore, { initialSignalRStoreState } from "../signalRStore";
import { mockHubBuilder } from "@/lib/testUtils/mocks/mockSignalR";

vi.mock("@microsoft/signalr");

describe("signalRStore", () => {
    const hubBuilderMock = signalR.HubConnectionBuilder as Mock;
    let hubBuilderInstanceMock: MockProxy<signalR.HubConnectionBuilder>;

    beforeEach(() => {
        useSignalRStore.setState(initialSignalRStoreState);
        hubBuilderInstanceMock = mockHubBuilder();
    });

    function renderSignalRStore() {
        return renderHook(() => useSignalRStore()).result.current;
    }

    describe("leaveHub", () => {
        it("should stop and remove the hub connection", async () => {
            const url = "test-url";
            const mockHub = mock<signalR.HubConnection>();
            hubBuilderInstanceMock.build.mockReturnValue(mockHub);

            const { getOrJoinHub, leaveHub } = renderSignalRStore();

            // Create the hub first
            await act(() => getOrJoinHub(url));

            expect(useSignalRStore.getState().hubs[url]).toBe(mockHub);

            // Now leave the hub
            await act(() => leaveHub(url));

            expect(mockHub.stop).toHaveBeenCalledOnce();
            expect(useSignalRStore.getState().hubs[url]).toBeUndefined();
        });
    });

    describe("getOrJoinHub", () => {
        it("should create a hub if it doesn't exist", async () => {
            const url = "test-url";
            const mockHub = mock<signalR.HubConnection>();
            hubBuilderInstanceMock.build.mockReturnValue(mockHub);

            const { getOrJoinHub } = renderSignalRStore();

            const result = await act(() => getOrJoinHub(url));
            expect(result).toBe(mockHub);

            expect(
                hubBuilderInstanceMock.withUrl,
            ).toHaveBeenCalledExactlyOnceWith(url);
            expect(
                hubBuilderInstanceMock.withAutomaticReconnect,
            ).toHaveBeenCalledOnce();
            expect(
                hubBuilderInstanceMock.configureLogging,
            ).toHaveBeenCalledExactlyOnceWith(signalR.LogLevel.Information);

            expect(hubBuilderInstanceMock.build).toHaveBeenCalledOnce();
            expect(hubBuilderInstanceMock.build).toHaveBeenCalledAfter(
                hubBuilderInstanceMock.withUrl,
            );
            expect(hubBuilderInstanceMock.build).toHaveBeenCalledAfter(
                hubBuilderInstanceMock.withAutomaticReconnect,
            );
            expect(hubBuilderInstanceMock.build).toHaveBeenCalledAfter(
                hubBuilderInstanceMock.configureLogging,
            );

            expect(hubBuilderMock).toHaveBeenCalledOnce();
        });

        it("should return the existing hub if it exists", async () => {
            const expectedHub = mock<signalR.HubConnection>();
            const otherHub = mock<signalR.HubConnection>();

            hubBuilderInstanceMock.build.mockReturnValue(expectedHub);

            const { getOrJoinHub } = renderSignalRStore();

            // First call to create the hub
            await act(() => getOrJoinHub("test-url"));

            hubBuilderInstanceMock.build.mockReturnValue(otherHub);

            // Second call should return the existing hub
            const result = await act(() => getOrJoinHub("test-url"));
            expect(result).toBe(expectedHub);
            expect(result).not.toBe(otherHub);
        });
    });
});
