import { useState } from "react";
import Link from "next/link";

import {
    useAuthedUser,
    useSessionUser,
} from "@/features/auth/hooks/useSessionUser";
import CountdownText from "@/components/CountdownText";
import { acceptChallenge, cancelChallenge, PoolType } from "@/lib/apiClient";
import Button from "@/components/ui/Button";
import constants from "@/lib/constants";
import { useRouter } from "next/navigation";
import useChallengeStore from "../../hooks/useChallengeStore";

const ChallengeFooter = () => {
    const [error, setError] = useState<string>();
    const [isInAction, setIsInAction] = useState(false);
    const user = useSessionUser();
    const router = useRouter();

    const { challenge, isCancelled, setCancelled, isExpired, setExpired } =
        useChallengeStore((x) => ({
            challenge: x.challenge,
            isCancelled: x.isCancelled,
            setCancelled: x.setCancelled,
            isExpired: x.isExpired,
            setExpired: x.setExpired,
        }));
    const expiresAt = new Date(challenge.expiresAt);

    async function onAccept() {
        setIsInAction(true);
        try {
            const { error, data: gameToken } = await acceptChallenge({
                path: { challengeId: challenge.challengeId },
            });
            if (error || gameToken === undefined) {
                console.error(error);
                setError("Failed to accept challenge");
                return;
            }

            router.push(`${constants.PATHS.GAME}/${gameToken}`);
        } finally {
            setIsInAction(false);
        }
    }

    async function onCancel() {
        setIsInAction(true);
        try {
            const { error } = await cancelChallenge({
                path: { challengeId: challenge.challengeId },
            });
            if (error) {
                console.error(error);
                setError("Failed to cancel challenge");
                return;
            }

            setCancelled(user?.userId ?? null);
        } finally {
            setIsInAction(false);
        }
    }

    if (isCancelled || isExpired) return <ChallengeOver />;

    return (
        <>
            {challenge.requester.userId === user?.userId ? (
                <Button
                    className="w-full"
                    onClick={onCancel}
                    disabled={isInAction}
                    data-testid="challengeFooterCancelButton"
                >
                    Cancel
                </Button>
            ) : (
                <RecipientActions
                    onAccept={onAccept}
                    onCancel={onCancel}
                    isInAction={isInAction}
                />
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

const RecipientActions = ({
    onAccept,
    onCancel,
    isInAction,
}: {
    onAccept: () => Promise<void>;
    onCancel: () => Promise<void>;
    isInAction: boolean;
}) => {
    const user = useAuthedUser();
    const challenge = useChallengeStore((x) => x.challenge);
    const isRatedAndGuest = challenge.pool.poolType === PoolType.RATED && !user;
    const isOpen = !challenge.recipient;

    return (
        <>
            <p
                className="text-xl font-medium"
                data-testid="challengeFooterRecipientPrompt"
            >
                Do you accept the challenge?
            </p>
            <div className="flex w-full gap-3">
                <Button
                    className="w-full"
                    disabled={isRatedAndGuest || isInAction}
                    onClick={onAccept}
                    data-testid="challengeFooterAcceptButton"
                >
                    {isRatedAndGuest
                        ? "Guests can't accept rated challenges"
                        : "Accept"}
                </Button>

                {!isOpen && (
                    <Button
                        className="w-full"
                        onClick={onCancel}
                        disabled={isInAction}
                        data-testid="challengeFooterDeclineButton"
                    >
                        Decline
                    </Button>
                )}
            </div>
        </>
    );
};

const ChallengeOver = () => {
    return (
        <Link
            href={constants.PATHS.PLAY}
            className="w-full"
            data-testid="challengeFooterNewOpponent"
        >
            <Button className="w-full">New Opponent</Button>
        </Link>
    );
};
