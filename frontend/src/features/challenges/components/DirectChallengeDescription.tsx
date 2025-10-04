import ProfilePicture from "@/features/profile/components/ProfilePicture";
import { MinimalProfile } from "@/lib/apiClient";

const DirectChallengeDescription = ({
    recipient,
}: {
    recipient: MinimalProfile;
}) => {
    return (
        <>
            <p className="text-2xl">Waiting For</p>
            <ProfilePicture
                userId={recipient.userId}
                className="animate-subtle-ping"
                width={200}
                height={200}
            />
            <p className="w-full truncate text-center text-xl">
                {recipient.userName}
            </p>
        </>
    );
};
export default DirectChallengeDescription;
