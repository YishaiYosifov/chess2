import { CheckIcon, XMarkIcon } from "@heroicons/react/24/outline";

import TimeControlIcon from "@/features/lobby/Components/TimeControlIcon";
import ProfileTooltip from "@/features/profile/components/ProfileTooltip";
import {
    acceptChallenge,
    cancelChallenge,
    ChallengeRequest,
    PoolType,
} from "@/lib/apiClient";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import { useState } from "react";
import { useRouter } from "next/navigation";
import constants from "@/lib/constants";

const ChallengeNotification = ({
    challenge,
    onRemove,
}: {
    challenge: ChallengeRequest;
    onRemove: (challengeId: string) => void;
}) => {
    const [isInAction, setIsInAction] = useState(false);
    const router = useRouter();

    async function onDecline() {
        setIsInAction(true);
        try {
            const { error } = await cancelChallenge({
                path: { challengeId: challenge.challengeId },
            });
            if (error) {
                console.error(error);
                return;
            }
            onRemove(challenge.challengeId);
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
                return;
            }

            onRemove(challenge.challengeId);
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
                <ProfileTooltip username={challenge.requester.userName}>
                    <p className="truncate">{challenge.requester.userName}</p>
                </ProfileTooltip>
                <p className="text-text/70 flex gap-1 text-sm">
                    <span>
                        {challenge.pool.timeControl.baseSeconds / 60}+
                        {challenge.pool.timeControl.incrementSeconds}
                    </span>
                    <span>
                        {challenge.pool.poolType === PoolType.RATED
                            ? "rated"
                            : "casual"}
                    </span>
                </p>
            </div>

            <Button
                className="bg-neutral-800 p-1"
                onClick={onDecline}
                disabled={isInAction}
            >
                <XMarkIcon className="h-9 w-9" />
            </Button>
            <Button
                className="bg-green-600 p-1"
                onClick={onAccept}
                disabled={isInAction}
            >
                <CheckIcon className="h-9 w-9" />
            </Button>
        </Card>
    );
};
export default ChallengeNotification;
