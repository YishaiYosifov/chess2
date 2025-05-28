import useSignalRStore from "@/stores/signalRStore";
import { HubConnectionState } from "@microsoft/signalr";
import { useCallback, useEffect } from "react";

export function useSignalREvent(hubUrl: string) {
    const signalRStore = useSignalRStore();

    // Get or join the SignalR hub
    const hubConnection = signalRStore.getOrJoinHub(hubUrl);

    // Ensure the hub connection is started
    useEffect(() => {
        if (hubConnection.state !== HubConnectionState.Connected) {
            hubConnection
                .start()
                .catch((err) =>
                    console.error("SignalR connection error:", err),
                );
        }
    }, [hubConnection]);

    // Function to send an event message
    const sendEventMessage = useCallback(
        (event: E, data: any) => {
            hubConnection
                .invoke(event, data)
                .catch((err) => console.error("SignalR send error:", err));
        },
        [hubConnection],
    );

    return { sendEventMessage };
}
