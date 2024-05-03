"use client";

import { useEffect } from "react";

import { useSharedWSEvent } from "@/hooks/useSharedWS";
import { useRouter } from "next/navigation";
import { WSEvent } from "@/models";

const WSPushAction = () => {
    const router = useRouter();
    const { data: gameStart } = useSharedWSEvent(WSEvent.GameStart);

    useEffect(() => {
        if (!gameStart) return;
        router.push(`/game/${gameStart.token}`);
    }, [gameStart, router]);

    return null;
};
export default WSPushAction;
