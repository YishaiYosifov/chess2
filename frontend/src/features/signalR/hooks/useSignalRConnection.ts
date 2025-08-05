import { HubConnection, HubConnectionState } from "@microsoft/signalr";
import { useEffect } from "react";

import useSignalRStore from "@/features/signalR/stores/signalRStore";
import { shallow } from "zustand/shallow";

const useSignalRConnection = (hubUrl: string): HubConnection | undefined => {
    const { connection, joinHub, setHubState } = useSignalRStore(
        (state) => ({
            connection: state.hubs.get(hubUrl),
            joinHub: state.joinHub,
            setHubState: state.setHubState,
        }),
        shallow,
    );

    useEffect(() => joinHub(hubUrl), [hubUrl, joinHub]);

    useEffect(() => {
        if (!connection) return;

        const handleClose = () =>
            setHubState(hubUrl, HubConnectionState.Disconnected);
        const handleReconnect = () =>
            setHubState(hubUrl, HubConnectionState.Connected);

        connection.on("ReceiveErrorAsync", console.error);
        connection.onclose(handleClose);
        connection.onreconnected(handleReconnect);

        if (connection.state === HubConnectionState.Disconnected) {
            connection
                .start()
                .then(() => {
                    setHubState(hubUrl, HubConnectionState.Connected);
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
    }, [connection, hubUrl, setHubState]);

    return connection;
};

export default useSignalRConnection;
