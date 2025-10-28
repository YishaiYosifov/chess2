import { CheckIcon, XMarkIcon } from "@heroicons/react/24/outline";
import { useRouter } from "next/navigation";
import { useState } from "react";

import {
    acceptChallenge,
    cancelChallenge,
    ChallengeRequest,
    PoolType,
} from "@/lib/apiClient";

import TimeControlIcon from "@/features/lobby/components/TimeControlIcon";
import ProfileTooltip from "@/features/profile/components/ProfileTooltip";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";

import constants from "@/lib/constants";

const ChallengeNotification = ({
    challenge,
    removeChallenge,
}: {
    challenge: ChallengeRequest;
    removeChallenge: (challengeId: string) => void;
}) => {
    const [isInAction, setIsInAction] = useState(false);
    const [error, setError] = useState<string>();
    const router = useRouter();

    async function onDecline() {
        setIsInAction(true);
        try {
            const { error } = await cancelChallenge({
                path: { challengeId: challenge.challengeId },
            });
            if (error) {
                console.error(error);
                setError("Failed to decline");
                return;
            }
            removeChallenge(challenge.challengeId);
        } finally {
            setIsInAction(false);
        }
    }

    async function onAccept() {
        setIsInAction(true);
        try {
            const { error, data: gameToken } = await acceptChallenge({
                path: { challengeId: challenge.challengeId },
            });
            if (error || gameToken === undefined) {
                console.error(error);
                setError("Failed to accept");
                return;
            }

            removeChallenge(challenge.challengeId);
            router.push(`${constants.PATHS.GAME}/${gameToken}`);
        } finally {
            setIsInAction(false);
        }
    }

    return (
        <Card
            className="w-100 max-w-full flex-row items-center"
            data-testid={`challengeNotification-${challenge.challengeId}`}
        >
            <TimeControlIcon
                timeControl={challenge.timeControl}
                className="h-9"
            />

            <div className="min-w-0 flex-1">
                <ProfileTooltip userId={challenge.requester.userId}>
                    <p
                        className="truncate"
                        data-testid="challengeNotificationUsername"
                    >
                        {challenge.requester.userName}
                    </p>
                </ProfileTooltip>

                <p className="text-text/70 flex gap-1 text-sm">
                    <span data-testid="challengeNotificationTimeControl">
                        {challenge.pool.timeControl.baseSeconds / 60}+
                        {challenge.pool.timeControl.incrementSeconds}
                    </span>
                    <span data-testid="challengeNotificationPoolType">
                        {challenge.pool.poolType === PoolType.RATED
                            ? "rated"
                            : "casual"}
                    </span>
                </p>

                {error && (
                    <span
                        className="text-error"
                        data-testid="challengeNotificationError"
                    >
                        {error}
                    </span>
                )}
            </div>

            <Button
                className="bg-neutral-900 p-1"
                onClick={onDecline}
                disabled={isInAction}
                data-testid="challengeNotificationDecline"
            >
                <XMarkIcon className="h-9 w-9" />
            </Button>
            <Button
                className="bg-green-600 p-1"
                onClick={onAccept}
                disabled={isInAction}
                data-testid="challengeNotificationAccept"
            >
                <CheckIcon className="h-9 w-9" />
            </Button>
        </Card>
    );
};
export default ChallengeNotification;
