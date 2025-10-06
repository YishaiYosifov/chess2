import { useState } from "react";
import Link from "next/link";

import { useAuthedUser } from "@/features/auth/hooks/useSessionUser";
import CountdownText from "@/components/CountdownText";
import { acceptChallenge, cancelChallenge, PoolType } from "@/lib/apiClient";
import Button from "@/components/ui/Button";
import constants from "@/lib/constants";
import { useRouter } from "next/navigation";
import useChallengeStore from "../hooks/useChallengeStore";

const ChallengeFooter = () => {
    const [error, setError] = useState<string>();
    const user = useAuthedUser();
    const router = useRouter();

    const { challenge, isCancelled, setCancelled, hasExpired, setExpired } =
        useChallengeStore((x) => ({
            challenge: x.challenge,
            isCancelled: x.isCancelled,
            setCancelled: x.setCancelled,
            hasExpired: x.hasExpired,
            setExpired: x.setExpired,
        }));
    const expiresAt = new Date(challenge.expiresAt + "Z");
    const isRequester = challenge.requester.userId === user?.userId;
    const isOpen = !challenge.recipient;
    const isRatedAndGuest = challenge.pool.poolType === PoolType.RATED && !user;

    async function handleAcceptChallenge() {
        const { error, data: gameToken } = await acceptChallenge({
            path: { challengeId: challenge.challengeId },
        });
        if (error || gameToken === undefined) {
            console.error(error);
            setError("Failed to accept challenge");
            return;
        }

        router.push(`${constants.PATHS.GAME}/${gameToken}`);
    }

    async function handleCancelChallenge() {
        const { error } = await cancelChallenge({
            path: { challengeId: challenge.challengeId },
        });
        if (error) {
            console.error(error);
            setError("Failed to cancel challenge");
            return;
        }
        setCancelled();
    }

    if (isCancelled || hasExpired)
        return (
            <Link
                href={constants.PATHS.PLAY}
                className="w-full"
                data-testid="challengeFooterNewOpponent"
            >
                <Button className="w-full">New Opponent</Button>
            </Link>
        );

    return (
        <>
            {isRequester ? (
                <Button className="w-full" onClick={handleCancelChallenge}>
                    Cancel
                </Button>
            ) : (
                <>
                    <p className="text-xl font-medium">
                        Do you accept the challenge?
                    </p>
                    <div className="flex w-full gap-3">
                        <Button
                            className="w-full"
                            disabled={isRatedAndGuest}
                            onClick={handleAcceptChallenge}
                        >
                            {isRatedAndGuest
                                ? "Guests can't accept rated challenges"
                                : "Accept"}
                        </Button>
                        {!isOpen && (
                            <Button
                                className="w-full"
                                onClick={handleCancelChallenge}
                            >
                                Decline
                            </Button>
                        )}
                    </div>
                </>
            )}

            {error && <span className="text-error">{error}</span>}

            <CountdownText
                getTimeUntil={() => expiresAt}
                onDateReached={setExpired}
            >
                {({ countdown }) => (
                    <p
                        className="text-text/70"
                        data-testid="challengeFooterExpiresIn"
                    >
                        Expires in {countdown}
                    </p>
                )}
            </CountdownText>
        </>
    );
};
export default ChallengeFooter;
