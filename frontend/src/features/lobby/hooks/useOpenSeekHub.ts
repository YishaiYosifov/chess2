import { signalREmitterHookFactory } from "@/features/signalR/hooks/useSignalREmitter";
import { signalREventHookFactory } from "@/features/signalR/hooks/useSignalREvent";
import constants from "@/lib/constants";
import { OpenSeek } from "../lib/types";
import { PoolKey } from "@/lib/apiClient";

export type OpenSeekClientEvents = {
    NewOpenSeeksAsync: [seeks: OpenSeek[]];
    OpenSeekEndedAsync: [userId: string, pool: PoolKey];
};

type OpenSeekHubEvents = {
    SubscribeAsync: [];
};

export const useOpenSeekEvent = signalREventHookFactory<OpenSeekClientEvents>(
    constants.SIGNALR_PATHS.OPENSEEK,
);
export const useOpenSeekEmitter = signalREmitterHookFactory<OpenSeekHubEvents>(
    constants.SIGNALR_PATHS.OPENSEEK,
);
