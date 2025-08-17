import {
    useLobbyEmitter,
    useLobbyEvent,
} from "@/features/signalR/hooks/useSignalRHubs";
import { useRef, useState } from "react";
import { PoolKeyToStr } from "../lib/matchmakingKeys";
import { PoolKey, PoolType } from "@/lib/apiClient";
import constants from "@/lib/constants";

export default function useMatchmaking(pool: PoolKey): {
    createSeek: () => Promise<void>;
    cancelSeek: () => Promise<void>;
    toggleSeek: () => Promise<void>;
    isSeeking: boolean;
} {
    const sendLobbyEvent = useLobbyEmitter();
    const [isSeeking, setIsSeeking] = useState(false);
    const poolKeyStr = PoolKeyToStr(pool);
    const resubscribeIntervalRef = useRef<NodeJS.Timeout>(null);

    useLobbyEvent("SeekFailedAsync", async (pool) => {
        if (PoolKeyToStr(pool) === poolKeyStr) resetSeekState();
    });

    async function createSeek(): Promise<void> {
        setIsSeeking(true);
        await sendSeekRequest();

        if (resubscribeIntervalRef.current)
            clearInterval(resubscribeIntervalRef.current);
        resubscribeIntervalRef.current = setInterval(
            sendSeekRequest,
            constants.SEEK_RESUBSCRIBE_INTERAVAL_MS,
        );
    }

    async function cancelSeek(): Promise<void> {
        await sendLobbyEvent("CancelSeekAsync", pool);
        resetSeekState();
    }

    async function sendSeekRequest() {
        switch (pool.poolType) {
            case PoolType.RATED:
                await sendLobbyEvent("SeekRatedAsync", pool.timeControl);
                break;
            case PoolType.CASUAL:
                await sendLobbyEvent("SeekCasualAsync", pool.timeControl);
                break;
            default:
                throw new Error(`Unknown pool type ${pool}`);
        }
    }

    function resetSeekState() {
        setIsSeeking(false);
        if (resubscribeIntervalRef.current)
            clearInterval(resubscribeIntervalRef.current);
    }

    function toggleSeek(): Promise<void> {
        if (isSeeking) return cancelSeek();
        else return createSeek();
    }

    return { createSeek, cancelSeek, toggleSeek, isSeeking };
}
