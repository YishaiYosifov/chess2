import { act } from "react";
import useSignalRStore from "../../stores/signalRStore";
import { renderHook } from "@testing-library/react";
import useHubState from "../useHubState";
import { HubConnectionState } from "@microsoft/signalr";

describe("useHubState", () => {
    const hubUrl = "https://test.com/hub";

    beforeEach(() => {
        useSignalRStore.setState(useSignalRStore.getInitialState());
    });

    it("should return Disconnected by default", () => {
        const { result } = renderHook(() => useHubState(hubUrl));
        expect(result.current).toBe(HubConnectionState.Disconnected);
    });

    it("should return updated state after setHubState is called", () => {
        const { result } = renderHook(() => useHubState(hubUrl));

        act(() => {
            useSignalRStore
                .getState()
                .setHubState(hubUrl, HubConnectionState.Connected);
        });

        expect(result.current).toBe(HubConnectionState.Connected);
    });

    it("should reflect multiple state transitions", () => {
        const { result } = renderHook(() => useHubState(hubUrl));

        act(() => {
            useSignalRStore
                .getState()
                .setHubState(hubUrl, HubConnectionState.Connecting);
        });
        expect(result.current).toBe(HubConnectionState.Connecting);

        act(() => {
            useSignalRStore
                .getState()
                .setHubState(hubUrl, HubConnectionState.Connected);
        });
        expect(result.current).toBe(HubConnectionState.Connected);

        act(() => {
            useSignalRStore
                .getState()
                .setHubState(hubUrl, HubConnectionState.Reconnecting);
        });
        expect(result.current).toBe(HubConnectionState.Reconnecting);

        act(() => {
            useSignalRStore
                .getState()
                .setHubState(hubUrl, HubConnectionState.Disconnected);
        });
        expect(result.current).toBe(HubConnectionState.Disconnected);
    });

    it("should unsubscribe when unmounted", () => {
        const { unmount } = renderHook(() => useHubState(hubUrl));
        unmount();

        expect(useSignalRStore.getState().listeners.size).toBe(0);
    });

    it("should manage multiple listeners per hub url", () => {
        const { unmount: unmount1 } = renderHook(() => useHubState(hubUrl));
        const { unmount: unmount2 } = renderHook(() => useHubState(hubUrl));

        const listeners = useSignalRStore.getState().listeners.get(hubUrl);
        expect(listeners?.size).toBe(2);

        unmount1();
        expect(useSignalRStore.getState().listeners.get(hubUrl)?.size).toBe(1);

        unmount2();
        expect(useSignalRStore.getState().listeners.has(hubUrl)).toBe(false);
    });

    it("should isolate state between different hub URLs", () => {
        const url1 = "https://test.com/hub1";
        const url2 = "https://test.com/hub2";

        const { result: result1 } = renderHook(() => useHubState(url1));
        const { result: result2 } = renderHook(() => useHubState(url2));

        act(() => {
            useSignalRStore
                .getState()
                .setHubState(url1, HubConnectionState.Connected);
        });

        act(() => {
            useSignalRStore
                .getState()
                .setHubState(url2, HubConnectionState.Reconnecting);
        });

        expect(result1.current).toBe(HubConnectionState.Connected);
        expect(result2.current).toBe(HubConnectionState.Reconnecting);
    });
});
