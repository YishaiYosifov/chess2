"use client";

import { StarIcon as StarIconOutline } from "@heroicons/react/24/outline";
import { StarIcon as StarIconSolid } from "@heroicons/react/24/solid";

import Card from "@/components/ui/Card";
import { useAuthedUser } from "@/features/auth/hooks/useSessionUser";
import {
    addStar,
    getStarredUsers,
    PagedResultOfMinimalProfile,
    removeStar,
} from "@/lib/apiClient";
import { useState } from "react";
import { RelationProfileRow } from "./RelationProfileRow";
import PaginatedItemsRenderer from "@/features/pagination/components/PaginatedItemsRenderer";

const StarsForm = ({
    initialStars,
}: {
    initialStars: PagedResultOfMinimalProfile;
}) => {
    const [stars, setStars] = useState(initialStars);
    const user = useAuthedUser();

    async function fetchStarsForPage(pageNumber: number): Promise<void> {
        if (!user) return;

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
    }

    async function star(userId: string): Promise<void> {
        const { error } = await addStar({
            path: { starredUserId: userId },
        });
        if (error) return console.error(error);
        setStars((prev) => ({ ...prev, totalCount: prev.totalCount + 1 }));
    }

    async function unstar(userId: string): Promise<void> {
        const { error } = await removeStar({
            path: { starredUserId: userId },
        });
        if (error) return console.error(error);
        setStars((prev) => ({ ...prev, totalCount: prev.totalCount - 1 }));
    }

    return (
        <Card className="gap-3">
            <h1 className="text-3xl" data-testid="starsFormHeading">
                Stars
            </h1>
            <p className="text-text/70" data-testid="starsFormCount">
                You starred {stars.totalCount} players
            </p>

            <PaginatedItemsRenderer
                page={stars.page}
                totalPages={stars.totalPages}
                items={stars.items}
                paginatedItem={(profile, i) => (
                    <RelationProfileRow
                        key={profile.userId}
                        index={i}
                        profile={profile}
                        activate={() => star(profile.userId)}
                        deactivate={() => unstar(profile.userId)}
                        buttonIcon={(isActive) =>
                            isActive ? (
                                <StarIconSolid
                                    className="h-8 w-8 text-amber-300"
                                    data-testid="starsFormStarIconSolid"
                                />
                            ) : (
                                <StarIconOutline
                                    className="h-8 w-8 text-amber-300"
                                    data-testid="starsFormStarIconOutline"
                                />
                            )
                        }
                        buttonLabel={(isActive) =>
                            isActive ? "Starred" : "Star"
                        }
                    />
                )}
                fetchItemsForPage={fetchStarsForPage}
            />
        </Card>
    );
};
export default StarsForm;
