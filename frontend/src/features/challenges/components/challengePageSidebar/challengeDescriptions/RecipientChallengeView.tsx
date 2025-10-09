import useChallengeStore from "@/features/challenges/hooks/useChallengeStore";
import ProfileTooltip from "@/features/profile/components/ProfileTooltip";
import ProfilePicture from "@/features/profile/components/ProfilePicture";
import ChallengeStatusText from "../ChallengeStatusText";

const RecipientChallengeView = () => {
    const requester = useChallengeStore((x) => x.challenge.requester);
    return (
        <>
            <ChallengeStatusText
                activeText="Challenged By"
                activeClassName="text-2xl"
                overClassName="text-error text-2xl"
            />

            <ProfilePicture userId={requester.userId} size={200} />
            <ProfileTooltip
                username={requester.userName}
                isAuthenticated={requester.isAuthenticated}
            >
                <p
                    data-testid="recipientChallengeViewUserName"
                    className="text-lg"
                >
                    {requester.userName}
                </p>
            </ProfileTooltip>
        </>
    );
};
export default RecipientChallengeView;
