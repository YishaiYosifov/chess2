"use client";

import {
    blockUser,
    getBlockedUsers,
    PagedResultOfMinimalProfile,
    unblockUser,
} from "@/lib/apiClient";

import PaginatedItemsRenderer from "@/features/pagination/components/PaginatedItemsRenderer";
import MinimalProfileAction from "@/features/profile/components/MinimalProfileAction";
import Card from "@/components/ui/Card";

const BlockedForm = ({
    initialBlocked,
}: {
    initialBlocked: PagedResultOfMinimalProfile;
}) => {
    async function block(userId: string): Promise<boolean> {
        const { error } = await blockUser({
            path: { blockedUserId: userId },
        });
        if (error) {
            console.error(error);
            return false;
        }
        return true;
    }

    async function unblock(userId: string): Promise<boolean> {
        const { error } = await unblockUser({
            path: { blockedUserId: userId },
        });
        if (error) {
            console.error(error);
            return false;
        }
        return true;
    }

    return (
        <PaginatedItemsRenderer
            initialPaged={initialBlocked}
            fetchItems={getBlockedUsers}
        >
            {({
                totalCount,
                items,
                incrementTotalCount,
                decrementTotalCount,
            }) => (
                <Card>
                    <h1 className="text-3xl">Blocked</h1>
                    <p className="text-text/70" data-testid="blockedFormCount">
                        You blocked {totalCount} players
                    </p>

                    {items.map((profile, i) => (
                        <MinimalProfileAction
                            key={profile.userId}
                            index={i}
                            profile={profile}
                            activate={async () =>
                                (await block(profile.userId)) &&
                                incrementTotalCount()
                            }
                            deactivate={async () =>
                                (await unblock(profile.userId)) &&
                                decrementTotalCount()
                            }
                            buttonLabel={(isActive) =>
                                isActive ? "Unblock" : "Block"
                            }
                        />
                    ))}
                </Card>
            )}
        </PaginatedItemsRenderer>
    );
};
export default BlockedForm;
