import { PoolKey, PoolType, TimeControl } from "@/lib/apiClient";

export type PoolKeyStr = `${PoolType}-${number}+${number}`;

export interface OpenSeek {
    userId: string;
    userName: string;
    pool: PoolKey;
    timeControl: TimeControl;
    rating?: number;
}

export type SeekKeyStr = `${PoolKeyStr}:${string}`;

export interface SeekKey {
    userId: string;
    pool: PoolKey;
}
