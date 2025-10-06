import constants from "@/lib/constants";
import { signalREventHookFactory } from "./useSignalREvent";
import { signalREmitterHookFactory } from "./useSignalREmitter";
import { PoolKey, TimeControlSettings } from "@/lib/apiClient";
import { OpenSeek } from "@/features/lobby/lib/types";

export type LobbyClientEvents = {
    MatchFoundAsync: [token: string];
    SeekFailedAsync: [pool: PoolKey];
};

type LobbyHubEvents = {
    SeekRatedAsync: [timeControl: TimeControlSettings];
    SeekCasualAsync: [timeControl: TimeControlSettings];
    CancelSeekAsync: [pool: PoolKey];
    CleanupConnectionAsync: [];

    MatchWithOpenSeekAsync: [userId: string, pool: PoolKey];
};

export const useLobbyEvent = signalREventHookFactory<LobbyClientEvents>(
    constants.SIGNALR_PATHS.LOBBY,
);

export const useLobbyEmitter = signalREmitterHookFactory<LobbyHubEvents>(
    constants.SIGNALR_PATHS.LOBBY,
);

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
