import { HubConnection } from "@microsoft/signalr";
import { useEffect } from "react";

import useSignalRStore from "@/features/signalR/stores/signalRStore";
import { shallow } from "zustand/shallow";

const useSignalRConnection = (hubUrl: string): HubConnection | undefined => {
    const { connection, joinHub, dereferenceHub } = useSignalRStore(
        (x) => ({
            connection: x.hubs.get(hubUrl)?.connection,
            joinHub: x.joinHub,
            dereferenceHub: x.dereferenceHub,
        }),
        shallow,
    );

    useEffect(() => {
        joinHub(hubUrl);
        return () => dereferenceHub(hubUrl);
    }, [hubUrl, joinHub, dereferenceHub]);

    return connection;
};

export default useSignalRConnection;
