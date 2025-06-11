import {
    HubConnection,
    HubConnectionBuilder,
    LogLevel,
} from "@microsoft/signalr";
import { createWithEqualityFn } from "zustand/traditional";

interface SignalRStore {
    hubs: Record<string, HubConnection>;
    joinHub: (url: string) => void;
    leaveHub: (url: string) => Promise<void>;
}

export const initialSignalRStoreState = {
    hubs: {},
};

const useSignalRStore = createWithEqualityFn<SignalRStore>((set, get) => ({
    ...initialSignalRStoreState,

    joinHub(url: string): void {
        const existingHub = get().hubs[url];
        if (existingHub) return;

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
    },

    async leaveHub(url: string) {
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
