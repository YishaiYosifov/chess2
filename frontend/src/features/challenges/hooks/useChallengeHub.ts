import useSignalREvent, {
    signalREventHookFactory,
} from "@/features/signalR/hooks/useSignalREvent";
import { ChallengeRequest } from "@/lib/apiClient";
import constants from "@/lib/constants";
import { useMemo } from "react";

export type ChallengeClientEvents = {
    ChallengeReceivedAsync: [challenge: ChallengeRequest];
    ChallengeCancelledAsync: [cancelledBy: string | null, challengeId: string];
    ChallengeAcceptedAsync: [gameToken: string, challengeId: string];
};

export const useChallengeEvent = signalREventHookFactory<ChallengeClientEvents>(
    constants.SIGNALR_PATHS.CHALLENGE,
);

export function useChallengeInstanceEvent<
    TEventName extends Extract<keyof ChallengeClientEvents, string>,
>(
    challengeId: string,
    eventName: TEventName,
    onEvent?: (...args: ChallengeClientEvents[TEventName]) => void,
) {
    const url = useMemo(() => {
        const u = new URL(constants.SIGNALR_PATHS.CHALLENGE);
        u.searchParams.append("challengeId", challengeId);
        return u.toString();
    }, [challengeId]);

    return useSignalREvent<ChallengeClientEvents, TEventName>(
        url,
        eventName,
        onEvent,
    );
}
