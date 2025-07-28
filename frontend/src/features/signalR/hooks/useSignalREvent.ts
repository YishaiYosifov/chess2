import { useEffect } from "react";

import useSignalRConnection from "./useSignalRConnection";

type EventMap = Record<string, unknown[]>;

export type EventHandlers<TEventMap extends EventMap> = {
    [K in keyof TEventMap]?: (...args: TEventMap[K]) => void;
};

function useSignalREvent<
    TEventMap extends EventMap,
    TEventName extends Extract<keyof TEventMap, string>,
>(
    hubUrl: string,
    eventName: TEventName,
    onEvent?: (...args: TEventMap[TEventName]) => void,
): void {
    const signalRConnection = useSignalRConnection(hubUrl);

    useEffect(() => {
        const handler = onEvent ?? (() => {});
        signalRConnection?.on(eventName, handler);

        return () => signalRConnection?.off(eventName, handler);
    }, [signalRConnection, eventName, onEvent]);
}
export default useSignalREvent;

export function signalREventHookFactory<TEventMap extends EventMap>(
    hubUrl: string,
) {
    const useSignalREventHook = <
        TEventName extends Extract<keyof TEventMap, string>,
    >(
        eventName: TEventName,
        onEvent?: (...args: TEventMap[TEventName]) => void,
    ) => useSignalREvent<TEventMap, TEventName>(hubUrl, eventName, onEvent);
    return useSignalREventHook;
}
