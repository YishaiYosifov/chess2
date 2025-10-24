import { StoreApi } from "zustand";
import { ChallengeStore } from "../stores/challengeStore";
import { useChallengeInstanceEvent } from "./useChallengeHub";
import { useRouter } from "next/navigation";
import constants from "@/lib/constants";

export default function useChallengeEvents(
    challengeStore: StoreApi<ChallengeStore>,
    challengeId: string,
) {
    const router = useRouter();

    useChallengeInstanceEvent(
        challengeId,
        "ChallengeAcceptedAsync",
        (gameToken, challengeId) => {
            const currentChallengeId =
                challengeStore.getState().challenge.challengeId;
            if (challengeId !== currentChallengeId) return;

            router.push(`${constants.PATHS.GAME}/${gameToken}`);
        },
    );

    useChallengeInstanceEvent(
        challengeId,
        "ChallengeCancelledAsync",
        (cancelledBy, challengeId) => {
            const { challenge: currentChallenge, setCancelled } =
                challengeStore.getState();
            if (challengeId !== currentChallenge.challengeId) return;
            setCancelled(cancelledBy);
        },
    );
}
