import { useState } from "react";
import Link from "next/link";

import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import CountdownText from "@/components/CountdownText";
import { ChallengeRequest } from "@/lib/apiClient";
import Button from "@/components/ui/Button";
import constants from "@/lib/constants";

const ChallengeFooter = ({ challenge }: { challenge: ChallengeRequest }) => {
    const user = useSessionUser();
    const [isExpired, setIsExpired] = useState(false);
    const expiresAt = new Date(challenge.expiresAt + "Z");
    const isRequester = challenge.requester.userId === user?.userId;

    if (isExpired)
        return (
            <>
                <p className="text-error text-xl">Challenge Expired!</p>
                <Link href={constants.PATHS.PLAY} className="w-full">
                    <Button className="w-full">New Opponent</Button>
                </Link>
            </>
        );

    return (
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
                    <p className="text-text/70">Expires in {countdown}</p>
                )}
            </CountdownText>
        </>
    );
};
export default ChallengeFooter;
