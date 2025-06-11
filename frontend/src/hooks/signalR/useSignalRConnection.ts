import { HubConnection, HubConnectionState } from "@microsoft/signalr";
import { useEffect } from "react";

import useSignalRStore from "@/stores/signalRStore";

const useSignalRConnection = (hubUrl: string): HubConnection | null => {
    const joinHub = useSignalRStore((state) => state.joinHub);
    const connection = useSignalRStore((state) => state.hubs[hubUrl]);

    useEffect(() => joinHub(hubUrl), [hubUrl, joinHub]);

    useEffect(() => {
        if (!connection) return;

        connection.on("ReceiveErrorAsync", console.error);
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
export default useSignalRConnection;
