import useSignalRConnection from "./useSignalRConnection";

const useSignalREmitter = <TEventMap extends Record<string, unknown[]>>(
    hubUrl: string,
) => {
    const connection = useSignalRConnection(hubUrl);

    const sendEvent = async <
        TEventName extends Extract<keyof TEventMap, string>,
    >(
        eventName: TEventName,
        ...args: TEventMap[TEventName]
    ): Promise<void> => connection?.invoke(eventName, ...args);

    return sendEvent;
};
export default useSignalREmitter;

export function signalREmitterHookFactory<
    TEventMap extends Record<string, unknown[]>,
>(hubUrl: string) {
    const useSignalREmitterHook = () => useSignalREmitter<TEventMap>(hubUrl);
    return useSignalREmitterHook;
}
