import {
    useLobbyEmitter,
    useLobbyEvent,
} from "@/features/signalR/hooks/useSignalRHubs";
import { TimeControlSettings } from "@/lib/apiClient";
import constants from "@/lib/constants";
import { useRouter } from "next/navigation";
import { useState } from "react";

export default function useMatchmaking(): {
    createSeek: (
        isRated: boolean,
        timeControl: TimeControlSettings,
    ) => Promise<void>;
    cancelSeek: () => Promise<void>;
    toggleSeek: (
        isRated: boolean,
        timeControl: TimeControlSettings,
    ) => Promise<void>;
    isSeeking: boolean;
} {
    const router = useRouter();
    const sendLobbyEvent = useLobbyEmitter();
    const [isSeeking, setIsSeeking] = useState(false);

    useLobbyEvent("MatchFoundAsync", (token) =>
        router.push(`${constants.PATHS.GAME}/${token}`),
    );
    useLobbyEvent("MatchFailedAsync", () => setIsSeeking(false));

    async function createSeek(
        isRated: boolean,
        timeControl: TimeControlSettings,
    ): Promise<void> {
        setIsSeeking(true);
        if (isRated) await sendLobbyEvent("SeekRatedAsync", timeControl);
        else await sendLobbyEvent("SeekCasualAsync", timeControl);
    }

    async function cancelSeek(): Promise<void> {
        setIsSeeking(false);
        await sendLobbyEvent("CancelSeekAsync");
    }

    function toggleSeek(
        isRated: boolean,
        timeControl: TimeControlSettings,
    ): Promise<void> {
        if (isSeeking) return cancelSeek();
        else return createSeek(isRated, timeControl);
    }

    return { createSeek, cancelSeek, toggleSeek, isSeeking };
}
