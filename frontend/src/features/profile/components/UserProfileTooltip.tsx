import { useFloating, flip, shift, autoUpdate } from "@floating-ui/react-dom";
import { useEffect, useState } from "react";
import clsx from "clsx";

import {
    CurrentRatingStatus,
    getCurrentRatings,
    getUserByUsername,
    PublicUser,
} from "@/lib/apiClient";

import TimeControlIcon from "@/features/lobby/Components/TimeControlIcon";
import ProfilePicture from "./ProfilePicture";
import constants from "@/lib/constants";
import Card from "@/components/ui/Card";
import Link from "next/link";
import Flag from "./Flag";

const UserProfileTooltip = ({
    username,
    children,
}: {
    username: string;
    children?: React.ReactNode;
}) => {
    const [profile, setProfile] = useState<PublicUser>();
    const [ratings, setRatings] = useState<CurrentRatingStatus[]>();
    const [open, setOpen] = useState(false);
    const hasLoaded = profile && ratings;

    const { refs, floatingStyles } = useFloating({
        open,
        middleware: [flip(), shift()],
        whileElementsMounted: autoUpdate,
    });

    async function loadProfile() {
        setOpen((prev) => !prev);

        let fetchedProfile: PublicUser | null = null;
        if (!profile) {
            const { error: profileError, data } = await getUserByUsername({
                path: { username },
            });
            if (profileError || data === undefined) {
                console.error(profileError);
                return;
            }
            setProfile(data);
            fetchedProfile = data;
        }

        if (!ratings && fetchedProfile) {
            const { error: ratingsError, data: ratings } =
                await getCurrentRatings({
                    path: { userId: fetchedProfile.userId },
                });
            if (ratingsError || !ratings) {
                console.error(ratingsError);
                return;
            }
            setRatings(ratings);
        }
    }

    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (
                refs.floating.current &&
                !refs.floating.current.contains(event.target as Node)
            ) {
                setOpen(false);
            }
        }

        if (open) document.addEventListener("mousedown", handleClickOutside);
        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, [open, refs.floating]);

    return (
        <>
            <div
                onClick={loadProfile}
                ref={refs.setReference}
                data-testid="userProfileTooltipChildren"
                className="flex min-w-0 cursor-pointer items-center gap-3"
            >
                {children}
            </div>

            {open && (
                <Card
                    ref={refs.setFloating}
                    style={floatingStyles}
                    className={clsx(
                        "bg-background z-50 mt-2 min-h-32 w-max max-w-[min(512px,100vw)] min-w-64",
                        !hasLoaded && "animate-lite-pulse",
                    )}
                    data-testid="userProfileTooltip"
                >
                    {hasLoaded && (
                        <ProfilePopupContent
                            profile={profile}
                            ratings={ratings}
                        />
                    )}
                </Card>
            )}
        </>
    );
};
export default UserProfileTooltip;

const ProfilePopupContent = ({
    profile,
    ratings,
}: {
    profile: PublicUser;
    ratings: CurrentRatingStatus[];
}) => {
    return (
        <>
            <Link
                className="flex w-full items-center gap-2"
                href={`${constants.PATHS.PROFILE}/${profile.userName}`}
                title={profile.userName}
                data-testid="userProfileTooltipLink"
            >
                <ProfilePicture userId={profile.userId} size={40} />

                <span
                    className="truncate"
                    data-testid="userProfileTooltipUsername"
                >
                    {profile.userName}
                </span>
                <Flag countryCode={profile.countryCode} size={20} />
            </Link>

            <p
                className="text-text/70 truncate text-sm"
                data-testid="userProfileTooltipAbout"
            >
                {profile.about}
            </p>

            <div className="flex gap-5">
                {ratings.map((rating, i) => (
                    <div
                        key={i}
                        className="flex flex-col items-center"
                        data-testid={`userProfileTooltipRating-${rating.timeControl}`}
                    >
                        <TimeControlIcon
                            timeControl={rating.timeControl}
                            className="h-10 w-10"
                        />
                        <p
                            data-testid={`userProfileTooltipRatingValue-${rating.timeControl}`}
                        >
                            {rating.rating}
                        </p>
                    </div>
                ))}
            </div>
        </>
    );
};
