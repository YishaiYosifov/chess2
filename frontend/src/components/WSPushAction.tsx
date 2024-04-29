"use client";

import { useSharedWSEvent, WSEvent } from "@/hooks/useSharedWS";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

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
