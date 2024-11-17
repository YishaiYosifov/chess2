"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

import { useEventWebSocket } from "@/hooks/useEventWS";
import { WSEventIn } from "@/lib/apiClient/models";

const WSPushAction = () => {
    const router = useRouter();
    const { lastData: gameStart } = useEventWebSocket(WSEventIn.GameStart);

    useEffect(() => {
        if (!gameStart) return;
        router.push(`/game/${gameStart.token}`);
    }, [gameStart, router]);

    return null;
};
export default WSPushAction;
