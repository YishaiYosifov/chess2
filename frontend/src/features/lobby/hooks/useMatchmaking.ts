import {
    useLobbyEmitter,
    useLobbyEvent,
} from "@/features/signalR/hooks/useSignalRHubs";
import { useState } from "react";
import { PoolKeyToStr } from "../lib/matchmakingKeys";
import { PoolKey, PoolType } from "@/lib/apiClient";

export default function useMatchmaking(pool: PoolKey): {
    createSeek: () => Promise<void>;
    cancelSeek: () => Promise<void>;
    toggleSeek: () => Promise<void>;
    isSeeking: boolean;
} {
    const sendLobbyEvent = useLobbyEmitter();
    const [isSeeking, setIsSeeking] = useState(false);
    const poolKeyStr = PoolKeyToStr(pool);

    useLobbyEvent("SeekFailedAsync", (pool) => {
        if (PoolKeyToStr(pool) === poolKeyStr) setIsSeeking(false);
    });

    async function createSeek(): Promise<void> {
        setIsSeeking(true);
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

    async function cancelSeek(): Promise<void> {
        setIsSeeking(false);
        await sendLobbyEvent("CancelSeekAsync", pool);
    }

    function toggleSeek(): Promise<void> {
        if (isSeeking) return cancelSeek();
        else return createSeek();
    }

    return { createSeek, cancelSeek, toggleSeek, isSeeking };
}
