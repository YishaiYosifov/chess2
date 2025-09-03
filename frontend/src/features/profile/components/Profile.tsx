"use client";

import { StarIcon as StarIconOutline } from "@heroicons/react/24/outline";
import { StarIcon as StarIconSolid } from "@heroicons/react/24/solid";

import ProfilePicture from "./ProfilePicture";
import Card from "@/components/ui/Card";
import {
    addStar,
    ApiProblemDetails,
    ProblemDetails,
    PublicUser,
    removeStar,
} from "@/lib/apiClient";
import Flag from "./Flag";
import Button from "@/components/ui/Button";
import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import Link from "next/link";
import constants from "@/lib/constants";
import { useState } from "react";

const Profile = ({
    profile,
    initialStarCount,
    initialHasStarred,
}: {
    profile: PublicUser;
    initialStarCount: number;
    initialHasStarred: boolean;
}) => {
    const loggedInUser = useSessionUser();
    const [hasStarred, setHasStarred] = useState(initialHasStarred);

    const createdAt = new Date(profile.createdAt);
    const formattedCreatedAt = createdAt.toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
    });

    async function toggleStar() {
        let error: ProblemDetails | ApiProblemDetails | undefined;
        if (hasStarred) {
            ({ error } = await removeStar({
                path: { starredUserId: profile.userId },
            }));
        } else {
            ({ error } = await addStar({
                path: { starredUserId: profile.userId },
            }));
        }

        if (error) console.error(error);
        else setHasStarred((prev) => !prev);
    }

    return (
        <Card className="flex flex-col gap-4 p-4 sm:flex-row sm:items-start">
            <ProfilePicture
                className="mx-auto sm:mx-0 sm:self-start"
                userId={profile.userId}
            />

            <section
                className="flex h-full min-w-0 flex-1 flex-col items-center justify-between gap-3
                    sm:items-start"
            >
                <div className="flex w-full flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                    <span
                        className="flex flex-1 items-center justify-center gap-3 overflow-hidden sm:justify-start"
                        data-testid="username"
                    >
                        <span
                            className="min-w-0 truncate text-2xl sm:text-3xl"
                            data-testid="profileUsername"
                        >
                            {profile.userName}
                        </span>
                        <Flag size={32} countryCode={profile.countryCode} />
                    </span>

                    <div className="flex flex-wrap gap-3 sm:gap-3">
                        {loggedInUser?.userId === profile.userId ? (
                            <Button className="flex-1">
                                <Link
                                    href={`/settings?${constants.SEARCH_PARAMS.SETTINGS_PAGE}=${constants.SETTING_QUERY_PATHS.PROFILE}`}
                                >
                                    Edit Profile
                                </Link>
                            </Button>
                        ) : (
                            <>
                                <Button
                                    className="flex min-w-[120px] flex-1 items-center justify-center gap-2"
                                    onClick={toggleStar}
                                    data-testid="profileStarButton"
                                >
                                    {hasStarred ? (
                                        <StarIconSolid className="h-8 w-8 text-amber-300" />
                                    ) : (
                                        <StarIconOutline className="h-8 w-8 text-amber-300" />
                                    )}
                                    {hasStarred ? "Unstar" : "Star"}
                                </Button>
                                <Button className="bg-secondary min-w-[100px] flex-1 text-black">
                                    Challenge
                                </Button>
                            </>
                        )}
                    </div>
                </div>

                <p
                    className="text-text/50 max-w-full text-sm break-words"
                    data-testid="aboutMe"
                >
                    {profile.about}
                </p>
                <div className="flex gap-3">
                    <p>
                        <span data-testid="profileStarCount">
                            {initialStarCount}
                        </span>{" "}
                        <span className="text-text/70">Stars</span>
                    </p>
                    <p>
                        <span>{formattedCreatedAt}</span>{" "}
                        <span className="text-text/70">Joined</span>
                    </p>
                </div>
            </section>
        </Card>
    );
};
export default Profile;
