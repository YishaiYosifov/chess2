import useWebSocket, { Options as WSOptions } from "react-use-websocket";
import { WebSocketHook } from "react-use-websocket/dist/lib/types";

import { WSEvent, WSEventMessageMap } from "@/models";

export type WSEventHook<E extends WSEvent> = WebSocketHook & {
    data: WSEventMessageMap[E] | null;
};

/**
 * Listen to a specific websocket event in the shared websocket instance
 *
 * @param event - which websocket event to listen to
 * @param options - the options to pass to the websocket
 * @returns the message when received
 */
export function useSharedWSEvent<E extends WSEvent>(
    event?: E,
    options?: WSOptions
): WSEventHook<E> {
    const wsHook = useSharedWSSilent({
        filter: (message) => {
            if (!event) return false;

            // filter messages that do not have the correct event
            const incomingEvent = message.data.split(":")[0];
            return incomingEvent == event;
        },
        ...options,
    });

    if (!wsHook.lastMessage) return { data: null, ...wsHook };

    // parse the data
    const data = wsHook.lastMessage.data.split(/:(.*)/s)[1];
    return { data: JSON.parse(data), ...wsHook };
}

/**
 * Use the shared websocket instance without receiving any events
 *
 * @param options - the options to pass to the websocket
 * @returns the useWebSocket hook data
 */
export function useSharedWSSilent(options?: WSOptions): WebSocketHook {
    return useWebSocket(process.env.NEXT_PUBLIC_WS_URL!, {
        shouldReconnect: () => true,
        reconnectInterval: 3000,
        share: true,
        filter: () => false,
        ...options,
    });
}
