import clsx from "clsx";

import useChallengeStore from "@/features/challenges/hooks/useChallengeStore";
import ProfilePicture from "@/features/profile/components/ProfilePicture";
import ProfileTooltip from "@/features/profile/components/ProfileTooltip";
import ChallengeStatusText from "../ChallengeStatusText";

const DirectChallengeView = () => {
    const { isOver, recipient } = useChallengeStore((x) => ({
        isOver: x.isCancelled || x.isExpired,
        recipient: x.challenge.recipient,
    }));
    if (!recipient) return null;

    return (
        <>
            <ChallengeStatusText
                activeText="Waiting For"
                activeClassName="text-2xl"
                overClassName="text-error text-2xl"
            />

            <ProfilePicture
                userId={recipient.userId}
                className={clsx(!isOver && "animate-subtle-ping")}
                size={200}
            />
            <ProfileTooltip
                username={recipient.userName}
                isAuthenticated={recipient.isAuthenticated}
            >
                <p
                    data-testid="directChallengeViewUserName"
                    className="text-lg"
                >
                    {recipient.userName}
                </p>
            </ProfileTooltip>
        </>
    );
};
export default DirectChallengeView;
