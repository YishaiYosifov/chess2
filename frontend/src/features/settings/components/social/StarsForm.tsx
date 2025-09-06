"use client";

import { StarIcon as StarIconOutline } from "@heroicons/react/24/outline";
import { StarIcon as StarIconSolid } from "@heroicons/react/24/solid";

import Card from "@/components/ui/Card";
import { useAuthedUser } from "@/features/auth/hooks/useSessionUser";
import PaginationStrip from "@/features/pagination/components/PaginationStrip";
import ProfilePicture from "@/features/profile/components/ProfilePicture";
import {
    addStar,
    getStarredUsers,
    MinimalProfile,
    PagedResultOfMinimalProfile,
    removeStar,
} from "@/lib/apiClient";
import clsx from "clsx";
import { useState } from "react";
import Button from "@/components/ui/Button";
import Link from "next/link";

const StarsForm = ({
    initialStars,
}: {
    initialStars: PagedResultOfMinimalProfile;
}) => {
    const [stars, setStars] = useState(initialStars);
    const [isFetching, setIsFetching] = useState(false);
    const user = useAuthedUser();

    async function fetchStarsForPage(pageNumber: number): Promise<void> {
        if (!user) return;

        try {
            setIsFetching(true);
            const { data: newStars, error } = await getStarredUsers({
                path: { userId: user.userId },
                query: {
                    Page: pageNumber,
                    PageSize: stars.pageSize,
                },
            });
            if (error || newStars === undefined) {
                console.error(error);
                return;
            }

            setStars(newStars);
        } finally {
            setIsFetching(false);
        }
    }

    async function addStarToPlayer(userId: string): Promise<void> {
        const { error } = await addStar({
            path: { starredUserId: userId },
        });
        if (error) return console.error(error);
    }

    async function removeStarFromPlayer(userId: string): Promise<void> {
        const { error } = await removeStar({
            path: { starredUserId: userId },
        });
        if (error) return console.error(error);
    }

    return (
        <Card className="gap-3" data-testid="starsFormRoot">
            <h1 className="text-3xl" data-testid="starsFormHeading">
                Stars
            </h1>
            <p className="text-text/70" data-testid="starsFormCount">
                You starred {stars.totalCount} players
            </p>

            {stars.items.map((profile, i) => (
                <StarProfile
                    key={profile.userId}
                    index={i}
                    profile={profile}
                    addStarToPlayer={addStarToPlayer}
                    removeStarFromPlayer={removeStarFromPlayer}
                />
            ))}
            <PaginationStrip
                currentPage={stars.page}
                totalPages={stars.totalPages}
                isFetching={isFetching}
                fetchItemsForPage={fetchStarsForPage}
            />
        </Card>
    );
};
export default StarsForm;

const StarProfile = ({
    index,
    profile,
    addStarToPlayer,
    removeStarFromPlayer,
}: {
    index: number;
    profile: MinimalProfile;
    addStarToPlayer: (userId: string) => Promise<void>;
    removeStarFromPlayer: (userId: string) => Promise<void>;
}) => {
    const [hasStarred, setHasStarred] = useState(true);
    const [isLoading, setIsLoading] = useState(false);

    async function toggleStar() {
        if (isLoading) return;

        try {
            setIsLoading(true);
            if (hasStarred) await removeStarFromPlayer(profile.userId);
            else await addStarToPlayer(profile.userId);
            setHasStarred((prev) => !prev);
        } finally {
            setIsLoading(false);
        }
    }

    return (
        <div
            key={profile.userId}
            data-testid={`starsFormStarProfile${profile.userId}`}
            className={clsx(
                "flex w-full items-center justify-between gap-3 rounded-md p-3",
                index % 2 == 0 ? "bg-white/5" : "bg-white/15",
            )}
        >
            <Link
                href={`/profile/${profile.userName}`}
                data-testid={`starsFormProfileLink${profile.userId}`}
            >
                <div className="flex items-center gap-3">
                    <ProfilePicture
                        userId={profile.userId}
                        width={80}
                        height={80}
                    />
                    <p className="text-lg">{profile.userName}</p>
                </div>
            </Link>

            <Button
                className="flex items-center gap-1"
                onClick={toggleStar}
                data-testid={`starsFormToggleStarButton${profile.userId}`}
            >
                {hasStarred ? (
                    <StarIconSolid
                        className="h-8 w-8 text-amber-300"
                        data-testid="starsFormStarIconSolid"
                    />
                ) : (
                    <StarIconOutline
                        className="h-8 w-8 text-amber-300"
                        data-testid="starsFormStarIconOutline"
                    />
                )}
                <span data-testid="starsFormStarButtonLabel">
                    {hasStarred ? "Starred" : "Star"}
                </span>
            </Button>
        </div>
    );
};
