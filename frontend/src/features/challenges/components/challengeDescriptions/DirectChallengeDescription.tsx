import clsx from "clsx";

import UserProfileTooltip from "@/features/profile/components/UserProfileTooltip";
import ProfilePicture from "@/features/profile/components/ProfilePicture";
import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import useChallengeStore from "../../hooks/useChallengeStore";
import { MinimalProfile } from "@/lib/apiClient";
import ChallengeStatusText from "../ChallengeStatusText";

const DirectChallengeDescription = () => {
    const challenge = useChallengeStore((x) => x.challenge);
    const user = useSessionUser();
    if (!challenge.recipient) return null;

    return challenge.requester.userId === user?.userId ? (
        <RequesterPOV recipient={challenge.recipient} />
    ) : (
        <RecipientPOV requester={challenge.requester} />
    );
};
export default DirectChallengeDescription;

const RequesterPOV = ({ recipient }: { recipient: MinimalProfile }) => {
    const isOver = useChallengeStore((x) => x.isCancelled || x.isExpired);
    return (
        <>
            <ChallengeStatusText
                activeText="Waiting For"
                activeClassName="text-2xl"
                overClassName="text-error text-2xl"
            />

            <ProfilePicture
                data-testid="directChallengeDescriptionProfilePicture"
                userId={recipient.userId}
                className={clsx(!isOver && "animate-subtle-ping")}
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

const RecipientPOV = ({ requester }: { requester: MinimalProfile }) => {
    return (
        <>
            <ChallengeStatusText
                activeText="Challenged By"
                activeClassName="text-2xl"
                overClassName="text-error text-2xl"
            />
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
