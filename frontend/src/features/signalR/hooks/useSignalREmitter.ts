import { HubConnectionState } from "@microsoft/signalr";
import { useCallback, useEffect, useRef } from "react";

import useSignalRConnection from "./useSignalRConnection";
import useSignalRStore from "../stores/signalRStore";

const useSignalREmitter = <TEventMap extends Record<string, unknown[]>>(
    hubUrl: string,
) => {
    const connection = useSignalRConnection(hubUrl);
    const state = useSignalRStore((x) => x.hubs.get(hubUrl)?.state);

    const connectionRef = useRef(connection);
    const pendingEventsRef = useRef<
        {
            eventName: Extract<keyof TEventMap, string>;
            args: TEventMap[keyof TEventMap];
        }[]
    >([]);

    useEffect(() => {
        connectionRef.current = connection;
        if (state !== HubConnectionState.Connected) return;

        for (const { eventName, args } of pendingEventsRef.current) {
            connection?.invoke(eventName, ...args);
        }
        pendingEventsRef.current = [];
    }, [state, connection]);

    const sendEvent = useCallback(
        async <TEventName extends Extract<keyof TEventMap, string>>(
            eventName: TEventName,
            ...args: TEventMap[TEventName]
        ): Promise<void> => {
            const connection = connectionRef.current;
            if (
                !connection ||
                connection.state !== HubConnectionState.Connected
            ) {
                pendingEventsRef.current.push({ eventName, args });
                return;
            }

            await connection.invoke(eventName, ...args);
        },
        [],
    );
    return sendEvent;
};
export default useSignalREmitter;

export function signalREmitterHookFactory<
    TEventMap extends Record<string, unknown[]>,
>(hubUrl: string) {
    const useSignalREmitterHook = () => useSignalREmitter<TEventMap>(hubUrl);
    return useSignalREmitterHook;
}
