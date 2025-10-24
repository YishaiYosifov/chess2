"use client";

import { StarIcon as StarIconOutline } from "@heroicons/react/24/outline";
import { StarIcon as StarIconSolid } from "@heroicons/react/24/solid";

import {
    addStar,
    getStarredUsers,
    PagedResultOfMinimalProfile,
    removeStar,
} from "@/lib/apiClient";

import PaginatedItemsRenderer from "@/features/pagination/components/PaginatedItemsRenderer";
import MinimalProfileAction from "@/features/profile/components/MinimalProfileAction";
import { useAuthedUser } from "@/features/auth/hooks/useSessionUser";
import Card from "@/components/ui/Card";

const StarsForm = ({
    initialStars,
}: {
    initialStars: PagedResultOfMinimalProfile;
}) => {
    const user = useAuthedUser();
    if (!user) return null;

    async function star(userId: string): Promise<boolean> {
        const { error } = await addStar({
            path: { starredUserId: userId },
        });
        if (error) {
            console.error(error);
            return false;
        }
        return true;
    }

    async function unstar(userId: string): Promise<boolean> {
        const { error } = await removeStar({
            path: { starredUserId: userId },
        });
        if (error) {
            console.error(error);
            return false;
        }
        return true;
    }

    return (
        <PaginatedItemsRenderer
            initialPaged={initialStars}
            fetchItems={({ query }) =>
                getStarredUsers({ path: { userId: user!.userId }, query })
            }
        >
            {({
                totalCount,
                items,
                incrementTotalCount,
                decrementTotalCount,
            }) => (
                <Card className="gap-3">
                    <h1 className="text-3xl" data-testid="starsFormHeading">
                        Stars
                    </h1>
                    <p className="text-text/70" data-testid="starsFormCount">
                        You starred {totalCount} players
                    </p>

                    {items.map((profile, i) => (
                        <MinimalProfileAction
                            key={profile.userId}
                            index={i}
                            profile={profile}
                            activate={async () =>
                                (await star(profile.userId)) &&
                                incrementTotalCount()
                            }
                            deactivate={async () =>
                                (await unstar(profile.userId)) &&
                                decrementTotalCount()
                            }
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
                    ))}
                </Card>
            )}
        </PaginatedItemsRenderer>
    );
};
export default StarsForm;
