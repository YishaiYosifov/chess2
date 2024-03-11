"use client";

import useWSEvent, { WSEvent } from "@/hooks/useWSEvent";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

const WSPushAction = () => {
    const router = useRouter();
    const gameStart = useWSEvent(WSEvent.GameStart);

    useEffect(() => {
        if (!gameStart) return;

        console.log(gameStart);
        router.push(`/game/${gameStart.token}`);
    }, [gameStart, router]);

    return null;
};
export default WSPushAction;
