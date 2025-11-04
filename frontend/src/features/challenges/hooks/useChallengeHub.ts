import useSignalREvent, {
    signalREventHookFactory,
} from "@/features/signalR/hooks/useSignalREvent";
import { ChallengeRequest } from "@/lib/apiClient";
import constants from "@/lib/constants";
import { useMemo } from "react";

export type ChallengeClientEvents = {
    ChallengeReceivedAsync: [challenge: ChallengeRequest];
    ChallengeCancelledAsync: [
        cancelledBy: string | null,
        challengeToken: string,
    ];
    ChallengeAcceptedAsync: [gameToken: string, challengeToken: string];
};

export const useChallengeEvent = signalREventHookFactory<ChallengeClientEvents>(
    constants.SIGNALR_PATHS.CHALLENGE,
);

export function useChallengeInstanceEvent<
    TEventName extends Extract<keyof ChallengeClientEvents, string>,
>(
    challengeToken: string,
    eventName: TEventName,
    onEvent?: (...args: ChallengeClientEvents[TEventName]) => void,
) {
    const url = useMemo(() => {
        const u = new URL(constants.SIGNALR_PATHS.CHALLENGE);
        u.searchParams.append("challengeToken", challengeToken);
        return u.toString();
    }, [challengeToken]);

    return useSignalREvent<ChallengeClientEvents, TEventName>(
        url,
        eventName,
        onEvent,
    );
}
