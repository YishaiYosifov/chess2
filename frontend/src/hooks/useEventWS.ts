import useWebSocket, { Options as WSOptions } from "react-use-websocket";
import { WebSocketHook } from "react-use-websocket/dist/lib/types";
import { useMemo } from "react";

import {
    WSEventIn,
    WSInEventMessageMap,
    WSEventOut,
    WSOutEventMessageMap,
} from "@/models";

export type SendEventMessageFunction = <T extends WSEventOut>(
    event: T,
    message: WSOutEventMessageMap[T],
    keep?: boolean
) => void;
type EventWebSocketHook<E extends WSEventIn> = WebSocketHook & {
    sendEventMessage: SendEventMessageFunction;
    lastData: WSInEventMessageMap[E] | null;
};

/**
 * Listen to a specific websocket event in the shared websocket instance
 *
 * @param event - which websocket event to listen to.
 *  if undefined, don't listen to any event
 * @param options - the options to pass to the websocket
 * @returns the message when received
 */
export function useEventWebSocket<E extends WSEventIn>(
    event?: E,
    options?: WSOptions
): EventWebSocketHook<E> {
    const wsHook = useWebSocket(process.env.NEXT_PUBLIC_WS_URL!, {
        shouldReconnect: () => true,
        reconnectInterval: 3000,
        share: true,
        filter: (message) => {
            if (!event || !message.data) return false;

            // filter messages that do not have the correct event
            const incomingEvent = message.data.split(":")[0];
            return incomingEvent == event;
        },
        ...options,
    });

    // parse the message every time we receive a new one
    const lastData = useMemo<WSInEventMessageMap[E] | null>(() => {
        if (!wsHook.lastMessage) return null;

        const data = wsHook.lastMessage.data.split(/:(.*)/s)[1];
        return JSON.parse(data);
    }, [wsHook.lastMessage]);

    const eventWsHook = wsHook as EventWebSocketHook<E>;
    eventWsHook.sendEventMessage = (event, data, keep) => {
        const wsMessage = `${event}:${JSON.stringify(data)}`;
        eventWsHook.sendMessage(wsMessage, keep);
    };

    return { ...eventWsHook, lastData };
}
