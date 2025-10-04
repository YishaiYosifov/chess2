import ProfilePicture from "@/features/profile/components/ProfilePicture";
import { MinimalProfile } from "@/lib/apiClient";

const ChallengeRecipientDescription = ({
    requester,
}: {
    requester: MinimalProfile;
}) => {
    return (
        <>
            <p className="text-2xl">Challenged By</p>
            <ProfilePicture
                userId={requester.userId}
                width={200}
                height={200}
            />
            <p className="w-full truncate text-center text-xl">
                {requester.userName}
            </p>
        </>
    );
};
export default ChallengeRecipientDescription;
