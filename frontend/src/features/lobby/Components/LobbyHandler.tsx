"use client";

import constants from "@/lib/constants";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useRef } from "react";
import useLobbyStore from "../stores/lobbyStore";
import { useLobbyEmitter, useLobbyEvent } from "../hooks/useLobbyHub";

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
        const { seeks, requestedOpenSeek, setRequestedOpenSeek, clearSeeks } =
            useLobbyStore.getState();
        if (requestedOpenSeek || seeks.size !== 0) {
            sendLobbyEvents("CleanupConnectionAsync");
            setRequestedOpenSeek(false);
            clearSeeks();
        }
    }, [pathname, sendLobbyEvents]);

    return null;
};
export default LobbyHandler;
