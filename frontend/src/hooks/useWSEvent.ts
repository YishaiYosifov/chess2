import useWebSocket from "react-use-websocket";

export const enum WSEvent {
    Notification = "notification",
    GameStart = "game_start",
}

interface Notification {
    someValue: boolean;
}

interface GameStart {
    otherValue: string;
}

type WSEventMessageMap = {
    [WSEvent.Notification]: Notification;
    [WSEvent.GameStart]: GameStart;
};

/**
 * a hook to listen for specific websocket events
 *
 * @param event - which websocket event to listen to
 * @returns the message when received
 */
export default function useWSEvent<E extends WSEvent>(
    event: E
): WSEventMessageMap[E] | null {
    const { lastMessage } = useWebSocket(process.env.NEXT_PUBLIC_WS_URL!, {
        shouldReconnect: () => true,
        reconnectInterval: 3000,
        share: true,
        filter: (message) => {
            // filter messages that do not have the correct event
            const incomingEvent = message.data.split(":")[0];
            return incomingEvent == event;
        },
    });

    if (!lastMessage) return null;

    // parse the data
    const data = lastMessage.data.split(/:(.*)/s)[1];
    return JSON.parse(data);
}
