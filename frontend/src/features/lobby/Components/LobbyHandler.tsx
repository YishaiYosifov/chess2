"use client";

import {
    useLobbyEmitter,
    useLobbyEvent,
} from "@/features/signalR/hooks/useSignalRHubs";
import constants from "@/lib/constants";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useRef } from "react";
import useLobbyStore from "../stores/lobbyStore";

const LobbyHandler = () => {
    const router = useRouter();
    const pathname = usePathname();
    const lastPathnameRef = useRef(pathname);

    const sendLobbyEvents = useLobbyEmitter();

    useLobbyEvent("MatchFoundAsync", (token) => {
        router.push(`${constants.PATHS.GAME}/${token}`);
    });

    useEffect(() => {
        if (lastPathnameRef.current === pathname) return;

        lastPathnameRef.current = pathname;
        const { seeks, clearSeeks } = useLobbyStore.getState();
        if (seeks.size !== 0) {
            sendLobbyEvents("CleanupConnectionAsync");
            clearSeeks();
        }
    }, [pathname, sendLobbyEvents]);

    return null;
};
export default LobbyHandler;
