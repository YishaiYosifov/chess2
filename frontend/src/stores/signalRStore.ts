import {
    HubConnection,
    HubConnectionBuilder,
    LogLevel,
} from "@microsoft/signalr";
import { createWithEqualityFn } from "zustand/traditional";

interface SignalRStore {
    hubs: Record<string, HubConnection>;
    getOrJoinHub: (url: string) => HubConnection;
    leaveHub: (url: string) => Promise<void>;
}

const useSignalRStore = createWithEqualityFn<SignalRStore>((set, get) => ({
    hubs: {},

    getOrJoinHub: (url: string) => {
        const existingHub = get().hubs[url];
        if (existingHub) return existingHub;

        const test = HubConnectionBuilder;
        const hubConnection = new HubConnectionBuilder()
            .withUrl(url)
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        set((state) => ({
            hubs: {
                ...state.hubs,
                [url]: hubConnection,
            },
        }));

        return hubConnection;
    },

    leaveHub: async (url: string) => {
        const hubConnection = get().hubs[url];
        if (!hubConnection) return;

        await hubConnection.stop();

        set((state) => {
            const newHubs = { ...state.hubs };
            delete newHubs[url];
            return { hubs: newHubs };
        });
    },
}));
export default useSignalRStore;
