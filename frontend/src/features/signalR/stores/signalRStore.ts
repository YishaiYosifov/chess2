import {
    HubConnection,
    HubConnectionBuilder,
    HubConnectionState,
    LogLevel,
} from "@microsoft/signalr";

import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";
import { enableMapSet, WritableDraft } from "immer";

interface Hub {
    connection: HubConnection;
    state: HubConnectionState;
    referenceCount: number;
}

interface SignalRStore {
    hubs: Map<string, Hub>;

    joinHub(url: string): void;
    dereferenceHub(url: string): void;
}

enableMapSet();
const useSignalRStore = createWithEqualityFn<SignalRStore>()(
    immer((set, get) => ({
        hubs: new Map(),

        joinHub(url) {
            if (typeof window === "undefined") return;

            const existingHub = get().hubs.get(url);
            if (existingHub) {
                set((state) => {
                    const hub = state.hubs.get(url);
                    if (hub) hub.referenceCount++;
                });
                return;
            }

            const hubConnection = new HubConnectionBuilder()
                .withUrl(url)
                .withAutomaticReconnect()
                .configureLogging(LogLevel.Information)
                .build();

            set((state) => {
                state.hubs.set(url, {
                    connection: hubConnection,
                    referenceCount: 1,
                    state: hubConnection.state,
                });
            });

            hubConnection.onclose(() => {
                updateHub(url, (hub) => {
                    hub.state = HubConnectionState.Disconnected;
                });
            });

            hubConnection.onreconnected(() => {
                updateHub(url, (hub) => {
                    hub.state = HubConnectionState.Connected;
                });
            });

            hubConnection.on("ReceiveErrorAsync", (err) => {
                console.error(`SignalR error from ${url}`, err);
            });

            hubConnection
                .start()
                .then(() => {
                    updateHub(url, (hub) => {
                        hub.state = HubConnectionState.Connected;
                    });
                })
                .catch(console.error);

            return hubConnection;
        },

        dereferenceHub(url) {
            const hub = get().hubs.get(url);
            if (!hub) return;

            const referenceCount = Math.max(0, hub.referenceCount - 1);
            if (
                referenceCount > 0 ||
                hub.state !== HubConnectionState.Connected
            ) {
                updateHub(url, (hub) => {
                    hub.referenceCount = referenceCount;
                });
                return;
            }

            hub.connection.stop().catch(console.error);

            set((state) => {
                state.hubs.delete(url);
            });
        },
    })),
    shallow,
);
export default useSignalRStore;

function updateHub(url: string, updater: (hub: WritableDraft<Hub>) => void) {
    useSignalRStore.setState((state) => {
        const hub = state.hubs.get(url);
        if (hub) updater(hub);
    });
}
