"use client";

import { StarIcon as StarIconOutline } from "@heroicons/react/24/outline";
import { StarIcon as StarIconSolid } from "@heroicons/react/24/solid";
import Link from "next/link";

import { PublicUser, SessionUser } from "@/lib/apiClient";
import { useSessionUser } from "@/features/auth/hooks/useSessionUser";
import useProfileActions from "../hooks/useProfileActions";
import ProfilePicture from "./ProfilePicture";
import Button from "@/components/ui/Button";
import constants from "@/lib/constants";
import Card from "@/components/ui/Card";
import Flag from "./Flag";
import ChallengePopup, {
    ChallengePopupRef,
} from "@/features/challenges/components/ChallengePopup";
import { useRef } from "react";

const Profile = ({
    profile,
    questPoints,
    initialStarCount,
    initialHasStarred,
    initialHasBlocked,
}: {
    profile: PublicUser;
    questPoints: number;
    initialStarCount: number;
    initialHasStarred: boolean;
    initialHasBlocked: boolean;
}) => {
    const loggedInUser = useSessionUser();
    const { hasStarred, starCount, hasBlocked, toggleStar, toggleBlock } =
        useProfileActions({
            profile,
            initialStarCount,
            initialHasStarred,
            initialHasBlocked,
        });
    const challengePopupRef = useRef<ChallengePopupRef>(null);

    const createdAt = new Date(profile.createdAt);
    const formattedCreatedAt = createdAt.toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
    });

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
                        <ProfileActions
                            loggedInUser={loggedInUser}
                            profile={profile}
                            hasStarred={hasStarred}
                            hasBlocked={hasBlocked}
                            toggleStar={toggleStar}
                            toggleBlock={toggleBlock}
                            challenge={() => challengePopupRef.current?.open()}
                        />
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
                        <span data-testid="profileStarCount">{starCount}</span>{" "}
                        <span className="text-text/70">Stars</span>
                    </p>
                    <p>
                        <span data-testid="profileQuestPoints">
                            {questPoints}
                        </span>{" "}
                        <span className="text-text/70">Quest Points</span>
                    </p>
                    <p>
                        <span data-testid="profileCreatedAt">
                            {formattedCreatedAt}
                        </span>{" "}
                        <span className="text-text/70">Joined</span>
                    </p>
                </div>
            </section>

            <ChallengePopup profile={profile} ref={challengePopupRef} />
        </Card>
    );
};
export default Profile;

const ProfileActions = ({
    loggedInUser,
    profile,
    hasStarred,
    hasBlocked,
    toggleStar,
    toggleBlock,
    challenge,
}: {
    loggedInUser: SessionUser | null;
    profile: PublicUser;
    hasStarred: boolean;
    hasBlocked: boolean;
    toggleStar: () => Promise<void>;
    toggleBlock: () => Promise<void>;
    challenge: () => void;
}) => {
    // guest user
    if (!loggedInUser) {
        return <ChallengeButton challenge={challenge} />;
    }

    // viewing your own profile
    if (loggedInUser.userId === profile.userId) {
        return (
            <Button className="flex-1">
                <Link
                    href={constants.PATHS.SETTINGS_PROFILE}
                    data-testid="editProfileLink"
                >
                    Edit Profile
                </Link>
            </Button>
        );
    }

    // logged in user viewing someone else's profile
    return (
        <div className="flex w-full flex-wrap gap-2">
            <Button
                className="flex flex-1 items-center justify-center gap-2"
                onClick={toggleStar}
                data-testid="profileStarButton"
            >
                {hasStarred ? (
                    <StarIconSolid className="h-6 w-6 text-amber-300" />
                ) : (
                    <StarIconOutline className="h-6 w-6 text-amber-300" />
                )}
                {hasStarred ? "Starred" : "Star"}
            </Button>
            <ChallengeButton challenge={challenge} />
            <Button
                className="flex-1 bg-neutral-800"
                onClick={toggleBlock}
                data-testid="profileBlockButton"
            >
                {hasBlocked ? "Unblock" : "Block"}
            </Button>
        </div>
    );
};

export const ChallengeButton = ({ challenge }: { challenge: () => void }) => {
    return (
        <Button
            className="flex-1"
            data-testid="profileChallengeButton"
            onClick={challenge}
        >
            Challenge
        </Button>
    );
};
