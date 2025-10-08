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
import RefreshRetryPolicy from "../lib/refreshRetryPolicy";
import ensureAuth from "@/features/auth/lib/ensureAuth";
import { ErrorCode } from "@/lib/apiClient";

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

            const hubConnection = createHubConnection(url);
            startHub(url, hubConnection);

            set((state) => {
                state.hubs.set(url, {
                    connection: hubConnection,
                    referenceCount: 1,
                    state: hubConnection.state,
                });
            });

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

function createHubConnection(url: string): HubConnection {
    const hubConnection = new HubConnectionBuilder()
        .withUrl(url)
        .withAutomaticReconnect(
            new RefreshRetryPolicy([1000, 2000, 5000], 20000),
        )
        .configureLogging(LogLevel.Information)
        .build();

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

    return hubConnection;
}

async function startHub(
    url: string,
    hubConnection: HubConnection,
): Promise<void> {
    try {
        await hubConnection.start();

        updateHub(url, (hub) => {
            hub.state = HubConnectionState.Connected;
        });
    } catch (err) {
        if (
            err instanceof Error &&
            err.message.includes(ErrorCode.AUTH_TOKEN_MISSING)
        ) {
            await ensureAuth();
        }

        setTimeout(() => startHub(url, hubConnection), 2000);
    }
}

function updateHub(
    url: string,
    updater: (hub: WritableDraft<Hub>) => void,
): void {
    useSignalRStore.setState((state) => {
        const hub = state.hubs.get(url);
        if (hub) updater(hub);
    });
}
