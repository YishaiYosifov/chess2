import { useEffect } from "react";

import useSignalRConnection from "./useSignalRConnection";

function useSignalREvent<
    TEventMap extends Record<string, unknown[]>,
    TEventName extends Extract<keyof TEventMap, string>,
>(
    hubUrl: string,
    eventName: TEventName,
    onEvent: (...args: TEventMap[TEventName]) => void,
): void {
    const signalRConnection = useSignalRConnection(hubUrl);

    useEffect(() => {
        signalRConnection?.on(eventName, onEvent);
    }, [signalRConnection, eventName, onEvent]);
}
export default useSignalREvent;

export function signalREventHookFactory<
    TEventMap extends Record<string, unknown[]>,
>(hubUrl: string) {
    const useSignalREventHook = <
        TEventName extends Extract<keyof TEventMap, string>,
    >(
        eventName: TEventName,
        onEvent: (...args: TEventMap[TEventName]) => void,
    ) => useSignalREvent<TEventMap, TEventName>(hubUrl, eventName, onEvent);
    return useSignalREventHook;
}
