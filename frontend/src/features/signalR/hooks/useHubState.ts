import { HubConnectionState } from "@microsoft/signalr";
import { useSyncExternalStore } from "react";

import useSignalRStore from "../stores/signalRStore";

export function useHubState(hubUrl: string): HubConnectionState {
    const subscribe = (cb: () => void) =>
        useSignalRStore.getState().subscribeToHubState(hubUrl, cb);

    const getSnapshot = () =>
        useSignalRStore.getState().hubStates.get(hubUrl) ??
        HubConnectionState.Disconnected;

    const getServerSnapshot = () => HubConnectionState.Disconnected;

    return useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot);
}
export default useHubState;
