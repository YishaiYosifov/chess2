import { useState } from "react";

import {
    addStar,
    blockUser,
    PublicUser,
    removeStar,
    unblockUser,
} from "@/lib/apiClient";

export default function useProfileActions({
    profile,
    initialStarCount,
    initialHasStarred,
    initialHasBlocked,
}: {
    profile: PublicUser;
    initialStarCount: number;
    initialHasStarred: boolean;
    initialHasBlocked: boolean;
}) {
    const [hasStarred, setHasStarred] = useState(initialHasStarred);
    const [starCount, setStarCount] = useState(initialStarCount);
    const [hasBlocked, setHasBlocked] = useState(initialHasBlocked);
    const [isInAction, setIsInAction] = useState(false);

    async function star(): Promise<void> {
        const { error } = await addStar({
            path: { starredUserId: profile.userId },
        });
        if (error) return console.error(error);

        setHasStarred(true);
        setStarCount((prev) => prev + 1);
    }

    async function unstar(): Promise<void> {
        const { error } = await removeStar({
            path: { starredUserId: profile.userId },
        });
        if (error) return console.error(error);

        setHasStarred(false);
        setStarCount((prev) => prev - 1);
    }

    async function toggleStar() {
        if (isInAction) return;
        setIsInAction(true);

        try {
            return await (hasStarred ? unstar() : star());
        } finally {
            setIsInAction(false);
        }
    }

    async function block(): Promise<void> {
        const { error } = await blockUser({
            path: { blockedUserId: profile.userId },
        });
        if (error) {
            console.error(error);
            return;
        }
        setHasBlocked(true);
    }

    async function unblock(): Promise<void> {
        const { error } = await unblockUser({
            path: { blockedUserId: profile.userId },
        });
        if (error) {
            console.error(error);
            return;
        }
        setHasBlocked(false);
    }

    async function toggleBlock() {
        if (isInAction) return;
        setIsInAction(true);

        try {
            return await (hasBlocked ? unblock() : block());
        } finally {
            setIsInAction(false);
        }
    }

    return { hasStarred, starCount, hasBlocked, toggleStar, toggleBlock };
}
