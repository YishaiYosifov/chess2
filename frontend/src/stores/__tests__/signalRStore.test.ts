import { renderHook } from "@testing-library/react";

import useSignalRStore from "../signalRStore";
import { mockSignalRConnectionBuilder } from "@/lib/testUtils/mocks";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { act } from "react";

vi.mock("@microsoft/signalr");

describe("signalRStore", () => {
    beforeEach(() => {
        useSignalRStore.getState().hubs = {};
    });

    function renderSignalRStore() {
        return renderHook(() => useSignalRStore()).result.current;
    }

    describe("leaveHub", () => {
        it("should stop and remove the hub connection", async () => {
            const builderMock = mockSignalRConnectionBuilder();
            const url = "test-url";
            const mockHub = { id: "mock-hub", stop: vi.fn() };
            builderMock.build.mockReturnValue(mockHub);

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
            const builderMock = mockSignalRConnectionBuilder();
            const url = "test-url";
            const mockHub = { id: "mock-hub" };
            builderMock.build.mockReturnValue(mockHub);

            const { getOrJoinHub } = renderSignalRStore();

            const result = await act(() => getOrJoinHub(url));
            expect(result).toBe(mockHub);

            expect(builderMock.withUrl).toHaveBeenCalledExactlyOnceWith(url);
            expect(builderMock.withAutomaticReconnect).toHaveBeenCalledOnce();
            expect(
                builderMock.configureLogging,
            ).toHaveBeenCalledExactlyOnceWith(LogLevel.Information);

            expect(builderMock.build).toHaveBeenCalledOnce();
            expect(builderMock.build).toHaveBeenCalledAfter(
                builderMock.withUrl,
            );
            expect(builderMock.build).toHaveBeenCalledAfter(
                builderMock.withAutomaticReconnect,
            );
            expect(builderMock.build).toHaveBeenCalledAfter(
                builderMock.configureLogging,
            );

            expect(HubConnectionBuilder).toHaveBeenCalledOnce();
        });

        it("should return the existing hub if it exists", async () => {
            const expectedHub = { id: "existing-hub" };
            const otherHub = { id: "other-hub" };

            const builderMock = mockSignalRConnectionBuilder();
            builderMock.build.mockReturnValue(expectedHub);

            const { getOrJoinHub } = renderSignalRStore();

            // First call to create the hub
            await act(() => getOrJoinHub("test-url"));

            builderMock.build.mockReturnValue(otherHub);

            // Second call should return the existing hub
            const result = await act(() => getOrJoinHub("test-url"));
            console.log(result);
            expect(result).toBe(expectedHub);
        });
    });
});
