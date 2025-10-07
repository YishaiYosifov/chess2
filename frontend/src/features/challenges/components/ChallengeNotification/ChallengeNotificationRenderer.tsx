"use client";

import { EyeDropperIcon } from "@heroicons/react/24/outline";
import { useState } from "react";

import { useChallengeEvent } from "../../hooks/useChallengeHub";
import ChallengeNotification from "./ChallengeNotification";
import { ChallengeRequest } from "@/lib/apiClient";
import Card from "@/components/ui/Card";

const ChallengeNotificationRenderer = () => {
    const [show, setShow] = useState(true);
    const [incomingChallenges, setIncomingChallenges] = useState<
        Map<string, ChallengeRequest>
    >(new Map());

    useChallengeEvent("ChallengeReceivedAsync", addChallenge);
    useChallengeEvent("ChallengeCancelledAsync", (_, challengeId) =>
        removeChallenge(challengeId),
    );

    function addChallenge(challenge: ChallengeRequest) {
        setIncomingChallenges((prev) => {
            const newChallenges = new Map(prev);
            newChallenges.set(challenge.challengeId, challenge);
            return newChallenges;
        });
    }

    function removeChallenge(challengeId: string) {
        setIncomingChallenges((prev) => {
            const newChallenges = new Map(prev);
            newChallenges.delete(challengeId);
            return newChallenges;
        });
    }
    if (incomingChallenges.size === 0) return null;

    const notificationCount =
        incomingChallenges.size > 9 ? "9+" : incomingChallenges.size;
    return (
        <div
            className="fixed right-10 bottom-10 z-50 flex flex-col items-end gap-y-1"
            onMouseEnter={() => setShow(true)}
            onMouseLeave={() => setShow(false)}
        >
            {show && (
                <div className="flex max-w-[calc(100vw-50px)] flex-col gap-y-1">
                    {[...incomingChallenges.values()].map((challenge) => (
                        <ChallengeNotification
                            key={challenge.challengeId}
                            challenge={challenge}
                            onRemove={removeChallenge}
                        />
                    ))}
                </div>
            )}

            <Card className="relative w-min cursor-pointer p-2">
                <EyeDropperIcon className="h-6 w-6" />
                <span
                    className="absolute -right-1.5 -bottom-1.5 flex h-5 w-5 items-center justify-center
                        rounded-full bg-red-500 text-sm"
                    data-testid="challengeNotificationRendererCount"
                >
                    {notificationCount}
                </span>
            </Card>
        </div>
    );
};
export default ChallengeNotificationRenderer;
