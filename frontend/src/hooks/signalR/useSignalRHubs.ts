import constants from "@/lib/constants";
import { signalREventHookFactory } from "./useSignalREvent";
import { signalREmitterHookFactory } from "./useSignalREmitter";
import { Point } from "@/types/tempModels";

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

type GameHubEvents = {
    MovePieceAsync: [gameToken: string, from: Point, to: Point];
};

export const useGameEmitter = signalREmitterHookFactory<GameHubEvents>(
    constants.SIGNALR_PATHS.GAME,
);
