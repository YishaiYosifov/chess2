import { renderHook } from "@testing-library/react";
import { mock, MockProxy } from "vitest-mock-extended";
import { Mock } from "vitest";
import { act } from "react";

import useSignalRStore from "../signalRStore";
import { mockHubBuilder } from "@/lib/testUtils/mocks/mockSignalR";
import {
    HubConnection,
    HubConnectionBuilder,
    LogLevel,
} from "@microsoft/signalr";

vi.mock("@microsoft/signalr");

describe("signalRStore", () => {
    const hubBuilderMock = HubConnectionBuilder as Mock;
    let hubBuilderInstanceMock: MockProxy<signalR.HubConnectionBuilder>;

    beforeEach(() => {
        useSignalRStore.setState(useSignalRStore.getInitialState());
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

            const { joinHub, leaveHub } = renderSignalRStore();

            // Create the hub first
            act(() => joinHub(url));

            expect(useSignalRStore.getState().hubs.get(url)).toBe(mockHub);

            // Now leave the hub
            await act(() => leaveHub(url));

            expect(mockHub.stop).toHaveBeenCalledOnce();
            expect(useSignalRStore.getState().hubs.get(url)).toBeUndefined();
        });
    });

    describe("joinHub", () => {
        it("should create a hub if it doesn't exist", async () => {
            const url = "test-url";
            const mockHub = mock<HubConnection>();
            hubBuilderInstanceMock.build.mockReturnValue(mockHub);

            const { joinHub } = renderSignalRStore();

            act(() => joinHub(url));
            expect(useSignalRStore.getState().hubs.get(url)).toBe(mockHub);

            expect(
                hubBuilderInstanceMock.withUrl,
            ).toHaveBeenCalledExactlyOnceWith(url);
            expect(
                hubBuilderInstanceMock.withAutomaticReconnect,
            ).toHaveBeenCalledOnce();
            expect(
                hubBuilderInstanceMock.configureLogging,
            ).toHaveBeenCalledExactlyOnceWith(LogLevel.Information);

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
            const expectedHub = mock<HubConnection>();
            const otherHub = mock<HubConnection>();

            hubBuilderInstanceMock.build.mockReturnValue(expectedHub);

            const { joinHub } = renderSignalRStore();

            // First call creates the hub
            act(() => joinHub("test-url"));
            expect(useSignalRStore.getState().hubs.get("test-url")).toBe(
                expectedHub,
            );

            hubBuilderInstanceMock.build.mockReturnValue(otherHub);

            // Second call should not create a new hub, so the stored one stays the same
            act(() => joinHub("test-url"));
            expect(useSignalRStore.getState().hubs.get("test-url")).toBe(
                expectedHub,
            );
            expect(useSignalRStore.getState().hubs.get("test-url")).not.toBe(
                otherHub,
            );
        });
    });
});
