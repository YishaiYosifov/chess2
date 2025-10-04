"use client";

import { useState } from "react";
import Link from "next/link";

import TimeControlIcon from "@/features/lobby/Components/TimeControlIcon";
import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import { ChallengeRequest, PoolType } from "@/lib/apiClient";
import CountdownText from "@/components/CountdownText";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import constants from "@/lib/constants";
import OpenChallengeDescription from "./OpenChallengeDescription";
import DirectChallengeDescription from "./DirectChallengeDescription";
import ChallengeRecipientDescription from "./ChallengeRecipientDescription";

const ChallengeSidebar = ({ challenge }: { challenge: ChallengeRequest }) => {
    const user = useSessionUser();

    const [isExpired, setIsExpired] = useState(false);

    const isRated = challenge.pool.poolType == PoolType.RATED;
    const expiresAt = new Date(challenge.expiresAt + "Z");
    const isRequester = challenge.requester.userId === user?.userId;

    return (
        <aside className="flex w-full min-w-xs flex-col gap-3 lg:max-w-sm">
            <Card className="items-center">
                <h1 className="text-4xl font-bold">Challenge</h1>

                <div className="flex items-center gap-2">
                    <TimeControlIcon
                        timeControl={challenge.timeControl}
                        className="h-8 w-8"
                    />
                    <p className="text-2xl font-semibold">
                        {challenge.pool.timeControl.baseSeconds / 60}+
                        {challenge.pool.timeControl.incrementSeconds}{" "}
                        {isRated ? "Rated" : "Casual"}
                    </p>
                </div>
            </Card>

            <Card className="flex-col items-center justify-center gap-5">
                {isRequester &&
                    (challenge.recipient ? (
                        <DirectChallengeDescription
                            recipient={challenge.recipient}
                        />
                    ) : (
                        <OpenChallengeDescription />
                    ))}
                {!isRequester && (
                    <ChallengeRecipientDescription
                        requester={challenge.requester}
                    />
                )}
            </Card>

            <Card className="items-center">
                {isExpired ? (
                    <>
                        <p className="text-error text-xl">Challenge Expired!</p>
                        <Link href={constants.PATHS.PLAY} className="w-full">
                            <Button className="w-full">New Opponent</Button>
                        </Link>
                    </>
                ) : (
                    <>
                        {isRequester ? (
                            <Button className="w-full">Cancel</Button>
                        ) : (
                            <>
                                <p className="text-xl font-medium">
                                    Do you accept the challenge?
                                </p>
                                <div className="flex w-full gap-3">
                                    <Button className="w-full">Accept</Button>
                                    <Button className="w-full">Decline</Button>
                                </div>
                            </>
                        )}

                        <CountdownText
                            getTimeUntil={() => expiresAt}
                            onDateReached={() => setIsExpired(true)}
                        >
                            {({ countdown }) => (
                                <p className="text-text/70">
                                    Expires in {countdown}
                                </p>
                            )}
                        </CountdownText>
                    </>
                )}
            </Card>
        </aside>
    );
};
export default ChallengeSidebar;
