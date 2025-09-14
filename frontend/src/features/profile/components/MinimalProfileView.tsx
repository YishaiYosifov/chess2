import { MinimalProfile } from "@/lib/apiClient";
import ProfilePicture from "./ProfilePicture";
import Link from "next/link";
import constants from "@/lib/constants";
import clsx from "clsx";

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
        >
            <Link
                href={`${constants.PATHS.PROFILE}/${profile.userName}`}
                className="flex min-w-0 items-center gap-3"
                data-testid="minimalProfileRowLink"
            >
                <ProfilePicture
                    userId={profile.userId}
                    width={80}
                    height={80}
                />
                <p
                    className="truncate text-lg"
                    data-testid="minimalProfileRowUsername"
                >
                    {profile.userName}
                </p>
            </Link>

            {children}
        </div>
    );
};
export default MinimalProfileView;
