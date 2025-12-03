import constants from "@/lib/constants";
import { PoolKey, TimeControlSettings } from "@/lib/apiClient";
import { signalREventHookFactory } from "@/features/signalR/hooks/useSignalREvent";
import { signalREmitterHookFactory } from "@/features/signalR/hooks/useSignalREmitter";
import { OngoingGame } from "../lib/types";

export type LobbyClientEvents = {
    MatchFoundAsync: [token: string];
    SeekFailedAsync: [pool: PoolKey];

    ReceiveOngoingGamesAsync: [games: OngoingGame[]];
    OngoingGameEndedAsync: [gameToken: string];
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
