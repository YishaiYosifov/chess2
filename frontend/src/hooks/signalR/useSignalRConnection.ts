import { HubConnection, HubConnectionState } from "@microsoft/signalr";
import { useEffect, useState } from "react";

import useSignalRStore from "@/stores/signalRStore";

const useSignalRConnection = (hubUrl: string): HubConnection | null => {
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
export default useSignalRConnection;
