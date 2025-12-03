import { MinimalProfile, PoolKey, PoolType } from "@/lib/apiClient";

export type PoolKeyStr = `${PoolType}-${number}+${number}`;

export interface OpenSeek {
    userId: string;
    userName: string;
    pool: PoolKey;
    rating?: number;
}

export type SeekKeyStr = `${PoolKeyStr}:${string}`;

export interface SeekKey {
    userId: string;
    pool: PoolKey;
}

export interface OngoingGame {
    gameToken: string;
    pool: PoolKey;
    opponent: MinimalProfile;
}
