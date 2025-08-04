import { HubConnection, HubConnectionState } from "@microsoft/signalr";
import { useEffect, useState } from "react";

import useSignalRStore from "@/features/signalR/stores/signalRStore";

const useSignalRConnection = (
    hubUrl: string,
): { connection: HubConnection | null; state: HubConnectionState } => {
    const joinHub = useSignalRStore((state) => state.joinHub);

    const connection = useSignalRStore((state) => state.hubs[hubUrl]);
    const [hubState, setHubState] = useState<HubConnectionState>(
        HubConnectionState.Disconnected,
    );

    useEffect(() => joinHub(hubUrl), [hubUrl, joinHub]);

    useEffect(() => {
        if (!connection) return;

        const handleClose = () => setHubState(HubConnectionState.Disconnected);
        const handleReconnect = () => setHubState(HubConnectionState.Connected);

        connection.on("ReceiveErrorAsync", console.error);
        connection.onclose(handleClose);
        connection.onreconnected(handleReconnect);

        if (connection.state === HubConnectionState.Disconnected) {
            connection
                .start()
                .then(() => {
                    setHubState(HubConnectionState.Connected);
                    console.log(`Connection started to ${hubUrl}`);
                })
                .catch((err) =>
                    console.error(`Connection failed to ${hubUrl}: `, err),
                );
        }

        return () => {
            connection.off("ReceiveErrorAsync", console.error);
            connection.off("close", handleClose);
            connection.off("reconnected", handleReconnect);
        };
    }, [connection, hubUrl]);
    return { connection, state: hubState };
};
export default useSignalRConnection;
