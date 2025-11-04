import { StoreApi } from "zustand";
import { ChallengeStore } from "../stores/challengeStore";
import { useChallengeInstanceEvent } from "./useChallengeHub";
import { useRouter } from "next/navigation";
import constants from "@/lib/constants";

export default function useChallengeEvents(
    challengeStore: StoreApi<ChallengeStore>,
    challengeToken: string,
) {
    const router = useRouter();

    useChallengeInstanceEvent(
        challengeToken,
        "ChallengeAcceptedAsync",
        (gameToken, challengeToken) => {
            const currentChallengeToken =
                challengeStore.getState().challenge.challengeToken;
            if (challengeToken !== currentChallengeToken) return;

            router.push(`${constants.PATHS.GAME}/${gameToken}`);
        },
    );

    useChallengeInstanceEvent(
        challengeToken,
        "ChallengeCancelledAsync",
        (cancelledBy, challengeToken) => {
            const { challenge: currentChallenge, setCancelled } =
                challengeStore.getState();
            if (challengeToken !== currentChallenge.challengeToken) return;
            setCancelled(cancelledBy);
        },
    );
}
