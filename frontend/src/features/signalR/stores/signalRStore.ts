import {
    HubConnection,
    HubConnectionBuilder,
    HubConnectionState,
    LogLevel,
} from "@microsoft/signalr";
import { enableMapSet } from "immer";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";
import { createWithEqualityFn } from "zustand/traditional";

interface SignalRStore {
    hubs: Map<string, HubConnection>;
    hubStates: Map<string, HubConnectionState>;
    listeners: Map<string, Set<() => void>>;

    joinHub: (url: string) => void;
    leaveHub: (url: string) => Promise<void>;
    setHubState: (url: string, hubState: HubConnectionState) => void;
    subscribeToHubState: (url: string, callback: () => void) => () => void;
}

enableMapSet();
const useSignalRStore = createWithEqualityFn<SignalRStore>()(
    immer((set, get) => ({
        hubs: new Map(),
        hubStates: new Map(),
        listeners: new Map(),

        joinHub(url: string): void {
            const existingHub = get().hubs.get(url);
            if (existingHub) return;

            const hubConnection = new HubConnectionBuilder()
                .withUrl(url)
                .withAutomaticReconnect()
                .configureLogging(LogLevel.Information)
                .build();

            set((state) => {
                state.hubs.set(url, hubConnection);
            });
        },

        async leaveHub(url: string) {
            const hubConnection = get().hubs.get(url);
            if (!hubConnection) return;

            await hubConnection.stop();

            set((state) => {
                state.hubs.delete(url);
            });
        },

        setHubState: (url, hubState) => {
            set((state) => {
                state.hubStates.set(url, hubState);
            });

            get()
                .listeners.get(url)
                ?.forEach((cb) => cb());
        },
        subscribeToHubState: (url, cb) => {
            set((state) => {
                const listeners = state.listeners.get(url) ?? new Set();
                listeners.add(cb);
                state.listeners.set(url, listeners);
            });

            return () => {
                set((state) => {
                    const listeners = state.listeners.get(url);
                    if (listeners) {
                        listeners.delete(cb);
                        if (listeners.size === 0) {
                            state.listeners.delete(url);
                        }
                    }
                });
            };
        },
    })),
    shallow,
);
export default useSignalRStore;
