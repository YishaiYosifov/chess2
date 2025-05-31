import constants from "@/lib/constants";
import { signalREventHookFactory } from "./useSignalREvent";
import { signalREmitterHookFactory } from "./useSignalREmitter";

type MatchmakingClientEvents = {
    MatchFoundAsync: [token: string];
};

type MatchmakingHubEvents = {
    SeekRatedAsync: [baseMinutes: number, increment: number];
    SeekCasualAsync: [baseMinutes: number, increment: number];
    CancelSeekAsync: [];
};

export const useMatchmakingEvent =
    signalREventHookFactory<MatchmakingClientEvents>(
        constants.SIGNALR_PATHS.MATCHMAKING,
    );

export const useMatchmakingEmitter =
    signalREmitterHookFactory<MatchmakingHubEvents>(
        constants.SIGNALR_PATHS.MATCHMAKING,
    );
