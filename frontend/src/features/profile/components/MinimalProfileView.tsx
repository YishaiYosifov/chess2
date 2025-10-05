import { MinimalProfile } from "@/lib/apiClient";
import ProfilePicture from "./ProfilePicture";
import clsx from "clsx";
import UserProfileTooltip from "./UserProfileTooltip";

const MinimalProfileView = ({
    profile,
    index = 0,
    children,
}: {
    profile: MinimalProfile;
    index?: number;
    children?: React.ReactNode;
}) => {
    return (
        <div
            className={clsx(
                "flex w-full min-w-0 flex-wrap items-center gap-3 rounded-md p-3",
                index % 2 === 0 ? "bg-white/5" : "bg-white/15",
            )}
            data-testid="minimalProfileRow"
        >
            <UserProfileTooltip username={profile.userName}>
                <ProfilePicture userId={profile.userId} size={80} />
                <p
                    className="truncate text-lg"
                    data-testid="minimalProfileRowUsername"
                >
                    {profile.userName}
                </p>
            </UserProfileTooltip>

            {children}
        </div>
    );
};
export default MinimalProfileView;
