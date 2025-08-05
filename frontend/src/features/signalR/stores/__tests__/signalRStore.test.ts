import { renderHook } from "@testing-library/react";
import { mock, MockProxy } from "vitest-mock-extended";
import { Mock } from "vitest";
import { act } from "react";

import useSignalRStore from "../signalRStore";
import { mockHubBuilder } from "@/lib/testUtils/mocks/mockSignalR";
import {
    HubConnection,
    HubConnectionBuilder,
    HubConnectionState,
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

    describe("subscribeToHubState", () => {
        const hubUrl = "test-hub";

        it("should add the callback to the listeners map", () => {
            const { subscribeToHubState } = renderSignalRStore();

            const callback = vi.fn();
            act(() => subscribeToHubState(hubUrl, callback));

            const listeners = useSignalRStore.getState().listeners.get(hubUrl);
            expect(listeners).toBeDefined();
            expect(listeners?.has(callback)).toBe(true);
            expect(listeners?.size).toBe(1);
        });

        it("should support multiple callbacks per URL", async () => {
            const { subscribeToHubState } = renderSignalRStore();

            const cb1 = vi.fn();
            const cb2 = vi.fn();

            await act(() => subscribeToHubState(hubUrl, cb1));
            await act(() => subscribeToHubState(hubUrl, cb2));

            const listeners = useSignalRStore.getState().listeners.get(hubUrl);
            expect(listeners?.size).toBe(2);
            expect(listeners?.has(cb1)).toBe(true);
            expect(listeners?.has(cb2)).toBe(true);
        });

        it("should remove callback on unsubscribe", async () => {
            const { subscribeToHubState } = renderSignalRStore();

            const cb = vi.fn();
            const unsubscribe = await act(() =>
                subscribeToHubState(hubUrl, cb),
            );

            let listeners = useSignalRStore.getState().listeners.get(hubUrl);
            expect(listeners?.has(cb)).toBe(true);

            act(() => unsubscribe());

            listeners = useSignalRStore.getState().listeners.get(hubUrl);
            expect(listeners).toBeUndefined();
        });

        it("should keep other callbacks when one is unsubscribed", async () => {
            const { subscribeToHubState } = renderSignalRStore();

            const cb1 = vi.fn();
            const cb2 = vi.fn();

            const unsub1 = await act(() => subscribeToHubState(hubUrl, cb1));
            await act(() => subscribeToHubState(hubUrl, cb2));

            act(() => unsub1());

            const listeners = useSignalRStore.getState().listeners.get(hubUrl);
            expect(listeners?.has(cb2)).toBe(true);
            expect(listeners?.has(cb1)).toBe(false);
        });
    });

    describe("setHubState", () => {
        const hubUrl = "test-hub";

        it("should update the hubStates map", async () => {
            const { setHubState } = renderSignalRStore();

            act(() => setHubState(hubUrl, HubConnectionState.Connected));

            const state = useSignalRStore.getState();
            expect(state.hubStates.get(hubUrl)).toBe(
                HubConnectionState.Connected,
            );
        });

        it("should notify subscribed listeners", async () => {
            const { setHubState, subscribeToHubState } = renderSignalRStore();

            const cb1 = vi.fn();
            const cb2 = vi.fn();

            await act(() => subscribeToHubState(hubUrl, cb1));
            await act(() => subscribeToHubState(hubUrl, cb2));

            act(() => setHubState(hubUrl, HubConnectionState.Connecting));

            expect(cb1).toHaveBeenCalledOnce();
            expect(cb2).toHaveBeenCalledOnce();
        });

        it("should not notify listeners of other hub URLs", async () => {
            const { setHubState, subscribeToHubState } = renderSignalRStore();

            const cb = vi.fn();
            await act(() => subscribeToHubState("other-hub", cb));

            act(() => setHubState(hubUrl, HubConnectionState.Reconnecting));

            expect(cb).not.toHaveBeenCalled();
        });
    });
});
