"use client";

import { useState } from "react";

import {
    blockUser,
    getBlockedUsers,
    PagedResultOfMinimalProfile,
    unblockUser,
} from "@/lib/apiClient";

import PaginatedItemsRenderer from "@/features/pagination/components/PaginatedItemsRenderer";
import { useAuthedUser } from "@/features/auth/hooks/useSessionUser";
import { RelationProfileRow } from "./RelationProfileRow";
import Card from "@/components/ui/Card";

const BlockedForm = ({
    initialBlocked,
}: {
    initialBlocked: PagedResultOfMinimalProfile;
}) => {
    const [blocked, setBlocked] = useState(initialBlocked);
    const user = useAuthedUser();

    async function fetchBlockedForPage(pageNumber: number): Promise<void> {
        if (!user) return;

        const { data: newBlocked, error } = await getBlockedUsers({
            query: {
                Page: pageNumber,
                PageSize: blocked.pageSize,
            },
        });
        if (error || newBlocked === undefined) {
            console.error(error);
            return;
        }

        setBlocked(newBlocked);
    }

    async function block(userId: string): Promise<void> {
        const { error } = await blockUser({
            path: { blockedUserId: userId },
        });
        if (error) return console.error(error);
        setBlocked((prev) => ({ ...prev, totalCount: prev.totalCount + 1 }));
    }

    async function unblock(userId: string): Promise<void> {
        const { error } = await unblockUser({
            path: { blockedUserId: userId },
        });
        if (error) return console.error(error);
        setBlocked((prev) => ({ ...prev, totalCount: prev.totalCount - 1 }));
    }

    return (
        <Card>
            <h1 className="text-3xl">Blocked</h1>
            <p className="text-text/70" data-testid="blockedFormCount">
                You blocked {blocked.totalCount} players
            </p>

            <PaginatedItemsRenderer
                page={blocked.page}
                totalPages={blocked.totalPages}
                items={blocked.items}
                paginatedItem={(profile, i) => (
                    <RelationProfileRow
                        key={profile.userId}
                        index={i}
                        profile={profile}
                        activate={() => block(profile.userId)}
                        deactivate={() => unblock(profile.userId)}
                        buttonLabel={(isActive) =>
                            isActive ? "Unblock" : "Block"
                        }
                    />
                )}
                fetchItemsForPage={fetchBlockedForPage}
            />
        </Card>
    );
};
export default BlockedForm;
