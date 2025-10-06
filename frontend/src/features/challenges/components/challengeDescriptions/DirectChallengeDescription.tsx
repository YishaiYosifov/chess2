import clsx from "clsx";

import UserProfileTooltip from "@/features/profile/components/UserProfileTooltip";
import ProfilePicture from "@/features/profile/components/ProfilePicture";
import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import useChallengeStore from "../../hooks/useChallengeStore";
import { MinimalProfile } from "@/lib/apiClient";

const DirectChallengeDescription = () => {
    const { challenge, isCancelled, hasExpired } = useChallengeStore((x) => ({
        challenge: x.challenge,
        isCancelled: x.isCancelled,
        hasExpired: x.hasExpired,
    }));
    const user = useSessionUser();
    if (!challenge.recipient) return null;

    return challenge.requester.userId === user?.userId ? (
        <RequesterPOV
            recipient={challenge.recipient}
            isCancelled={isCancelled}
            hasExpired={hasExpired}
        />
    ) : (
        <RecipientPOV
            requester={challenge.requester}
            isCancelled={isCancelled}
            hasExpired={hasExpired}
        />
    );
};
export default DirectChallengeDescription;

const RequesterPOV = ({
    recipient,
    isCancelled,
    hasExpired,
}: {
    recipient: MinimalProfile;
    isCancelled: boolean;
    hasExpired: boolean;
}) => {
    const isOver = isCancelled || hasExpired;
    return (
        <>
            {isOver ? (
                <p
                    data-testid="directChallengeDescriptionTitle"
                    className="text-error text-2xl"
                >
                    Challenge {isCancelled ? "Cancelled" : "Expired"}
                </p>
            ) : (
                <p
                    data-testid="directChallengeDescriptionTitle"
                    className={"text-text/70 text-2xl"}
                >
                    Waiting For
                </p>
            )}

            <ProfilePicture
                data-testid="directChallengeDescriptionProfilePicture"
                userId={recipient.userId}
                className={clsx(
                    !isCancelled && !hasExpired && "animate-subtle-ping",
                )}
                size={200}
            />
            <UserProfileTooltip username={recipient.userName}>
                <p
                    data-testid="directChallengeDescriptionUserName"
                    className="text-lg"
                >
                    {recipient.userName}
                </p>
            </UserProfileTooltip>
        </>
    );
};

const RecipientPOV = ({
    requester,
    isCancelled,
    hasExpired,
}: {
    requester: MinimalProfile;
    isCancelled: boolean;
    hasExpired: boolean;
}) => {
    const isOver = isCancelled || hasExpired;
    return (
        <>
            {isOver ? (
                <p
                    data-testid="directChallengeDescriptionTitle"
                    className="text-error text-2xl"
                >
                    Challenge {isCancelled ? "Cancelled" : "Expired"}
                </p>
            ) : (
                <p
                    data-testid="directChallengeDescriptionTitle"
                    className={"text-text/70 text-2xl"}
                >
                    Challenged By{" "}
                </p>
            )}
            <ProfilePicture userId={requester.userId} size={200} />
            <UserProfileTooltip username={requester.userName}>
                <p
                    data-testid="challengeRecipientDescriptionUserName"
                    className="text-lg"
                >
                    {requester.userName}
                </p>
            </UserProfileTooltip>
        </>
    );
};
