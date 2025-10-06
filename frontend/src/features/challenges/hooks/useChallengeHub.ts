import { signalREventHookFactory } from "@/features/signalR/hooks/useSignalREvent";
import { ChallengeRequest } from "@/lib/apiClient";
import constants from "@/lib/constants";

export type ChallengeClientEvents = {
    ChallengeReceivedAsync: [challenge: ChallengeRequest];
    ChallengeCancelledAsync: [cancelledBy: string, challengeId: string];
    ChallengeAcceptedAsync: [gameToken: string, challengeId: string];
};

export const useChallengeEvent = signalREventHookFactory<ChallengeClientEvents>(
    constants.SIGNALR_PATHS.CHALLENGE,
);
