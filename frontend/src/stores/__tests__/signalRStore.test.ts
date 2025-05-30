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
    let hubBuilderMethodMocks: MockProxy<signalR.HubConnectionBuilder>;

    beforeEach(() => {
        useSignalRStore.setState(initialSignalRStoreState);
        hubBuilderMethodMocks = mockHubBuilder();
    });

    function renderSignalRStore() {
        return renderHook(() => useSignalRStore()).result.current;
    }

    describe("leaveHub", () => {
        it("should stop and remove the hub connection", async () => {
            const url = "test-url";
            const mockHub = mock<signalR.HubConnection>();
            hubBuilderMethodMocks.build.mockReturnValue(mockHub);

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
            hubBuilderMethodMocks.build.mockReturnValue(mockHub);

            const { getOrJoinHub } = renderSignalRStore();

            const result = await act(() => getOrJoinHub(url));
            expect(result).toBe(mockHub);

            expect(
                hubBuilderMethodMocks.withUrl,
            ).toHaveBeenCalledExactlyOnceWith(url);
            expect(
                hubBuilderMethodMocks.withAutomaticReconnect,
            ).toHaveBeenCalledOnce();
            expect(
                hubBuilderMethodMocks.configureLogging,
            ).toHaveBeenCalledExactlyOnceWith(signalR.LogLevel.Information);

            expect(hubBuilderMethodMocks.build).toHaveBeenCalledOnce();
            expect(hubBuilderMethodMocks.build).toHaveBeenCalledAfter(
                hubBuilderMethodMocks.withUrl,
            );
            expect(hubBuilderMethodMocks.build).toHaveBeenCalledAfter(
                hubBuilderMethodMocks.withAutomaticReconnect,
            );
            expect(hubBuilderMethodMocks.build).toHaveBeenCalledAfter(
                hubBuilderMethodMocks.configureLogging,
            );

            expect(hubBuilderMock).toHaveBeenCalledOnce();
        });

        it("should return the existing hub if it exists", async () => {
            const expectedHub = mock<signalR.HubConnection>();
            const otherHub = mock<signalR.HubConnection>();

            hubBuilderMethodMocks.build.mockReturnValue(expectedHub);

            const { getOrJoinHub } = renderSignalRStore();

            // First call to create the hub
            await act(() => getOrJoinHub("test-url"));

            hubBuilderMethodMocks.build.mockReturnValue(otherHub);

            // Second call should return the existing hub
            const result = await act(() => getOrJoinHub("test-url"));
            expect(result).toBe(expectedHub);
            expect(result).not.toBe(otherHub);
        });
    });
});
