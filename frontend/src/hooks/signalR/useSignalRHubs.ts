import constants from "@/lib/constants";
import { signalREventHookFactory } from "./useSignalREvent";
import { signalREmitterHookFactory } from "./useSignalREmitter";

type MatchmakingClientEvents = {
    TestClient: [a: string];
};

type MatchmakingHubEvents = {
    TestHub: [a: string];
};

export const useMatchmakingEvent =
    signalREventHookFactory<MatchmakingClientEvents>(
        constants.SIGNALR_PATHS.MATCHMAKING,
    );

export const useMatchmakingEmitter =
    signalREmitterHookFactory<MatchmakingHubEvents>(
        constants.SIGNALR_PATHS.MATCHMAKING,
    );
