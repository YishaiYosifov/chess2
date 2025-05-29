import { useEffect, useRef, useState } from "react";

import useSignalRStore from "@/stores/signalRStore";
import constants from "@/lib/constants";
import { HubConnection, HubConnectionState } from "@microsoft/signalr";

type MatchmakingClientEvents = {
    TestClient: [a: string];
};

export const useSignalRConnection = (hubUrl: string): HubConnection | null => {
    const [connection, setConnection] = useState<HubConnection | null>(null);
    const signalRStore = useSignalRStore();

    useEffect(() => {
        const newConnection = signalRStore.getOrJoinHub(hubUrl);
        setConnection(newConnection);
    }, [hubUrl, signalRStore]);

    useEffect(() => {
        if (!connection) return;

        if (connection.state === HubConnectionState.Disconnected) {
            connection
                .start()
                .then(() => {
                    console.log(`Connection started to ${hubUrl}`);
                })
                .catch((err) =>
                    console.error(`Connection failed to ${hubUrl}: `, err),
                );
        }
    }, [connection, hubUrl]);

    return connection;
};

export const useSignalRHubEvent = <TEventMap extends Record<string, unknown[]>>(
    hubUrl: string,
) => {
    const connection = useSignalRConnection(hubUrl);
    const sendEvent = <TEventName extends Extract<keyof TEventMap, string>>(
        eventName: TEventName,
        ...args: TEventMap[TEventName]
    ) => {
        connection?.invoke(eventName, ...args);
    };
    return sendEvent;
};

export const useMatchmakingEvent = <
    TEventName extends Extract<keyof MatchmakingClientEvents, string>,
>(
    eventName: TEventName,
    onEvent: (...args: MatchmakingClientEvents[TEventName]) => void,
) =>
    useSignalREvent<MatchmakingClientEvents, TEventName>(
        constants.WEBSOCKET_PATHS.MATCHMAKING,
        eventName,
        onEvent,
    );

export function useSignalREvent<
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
    }, [hubUrl, signalRConnection, eventName, onEvent]);
}
