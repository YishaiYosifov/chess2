import useSignalRConnection from "./useSignalRConnection";

const useSignalREmitter = <TEventMap extends Record<string, unknown[]>>(
    hubUrl: string,
) => {
    const connection = useSignalRConnection(hubUrl);
    const sendEvent = <TEventName extends Extract<keyof TEventMap, string>>(
        eventName: TEventName,
        ...args: TEventMap[TEventName]
    ) => {
        connection?.invoke(eventName, ...args);
    };
    return sendEvent;
};
export default useSignalREmitter;

export function signalREmitterHookFactory<
    TEventMap extends Record<string, unknown[]>,
>(hubUrl: string) {
    const useSignalREmitterHook = () => useSignalREmitter<TEventMap>(hubUrl);
    return useSignalREmitterHook;
}
