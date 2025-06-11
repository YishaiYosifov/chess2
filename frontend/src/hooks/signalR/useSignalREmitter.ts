import { useEffect, useRef } from "react";
import useSignalRConnection from "./useSignalRConnection";

const useSignalREmitter = <TEventMap extends Record<string, unknown[]>>(
    hubUrl: string,
) => {
    const connection = useSignalRConnection(hubUrl);
    const connectionRef = useRef(connection);
    useEffect(() => {
        connectionRef.current = connection;
    }, [connection]);

    async function sendEvent<
        TEventName extends Extract<keyof TEventMap, string>,
    >(eventName: TEventName, ...args: TEventMap[TEventName]): Promise<void> {
        if (!connectionRef.current) {
            console.warn("No connection available yet");
            return;
        }
        await connectionRef.current.invoke(eventName, ...args);
    }
    return sendEvent;
};
export default useSignalREmitter;

export function signalREmitterHookFactory<
    TEventMap extends Record<string, unknown[]>,
>(hubUrl: string) {
    const useSignalREmitterHook = () => useSignalREmitter<TEventMap>(hubUrl);
    return useSignalREmitterHook;
}
