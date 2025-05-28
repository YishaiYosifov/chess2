"use client";

// import { useRouter } from "next/navigation";
// import { useEffect } from "react";

// import { useEventWebSocket } from "@/hooks/useEventWS";
// import { WSEventIn } from "@/lib/apiClient/models";

import * as signalR from "@microsoft/signalr";
import { useEffect, useState } from "react";

const WSPushAction = () => {
    const [messages, setMessages] = useState<string[]>([]);

    useEffect(() => {
        console.log("test test 123");
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:7266/api/ws/matchmaking")
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        connection.start().then(() => {
            console.log("Connection started");
        });
    }, []);

    return <></>;
    // const router = useRouter();
    // const { lastData: gameStart } = useEventWebSocket(WSEventIn.GameStart);
    // useEffect(() => {
    //     if (!gameStart) return;
    //     router.push(`/game/${gameStart.token}`);
    // }, [gameStart, router]);
    // return null;
};
export default WSPushAction;
