import { PoolKey } from "@/lib/apiClient";
import { SeekKeyStr, SeekKey, PoolKeyStr } from "./types";

export function SeekKeyToStr(seekKey: SeekKey): SeekKeyStr {
    return `${PoolKeyToStr(seekKey.pool)}:${seekKey.userId}`;
}

export function PoolKeyToStr(poolKey: PoolKey): PoolKeyStr {
    return `${poolKey.poolType}-${poolKey.timeControl.baseSeconds}+${poolKey.timeControl.incrementSeconds}`;
}
