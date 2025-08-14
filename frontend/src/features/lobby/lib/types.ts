import { TimeControl, TimeControlSettings } from "@/lib/apiClient";

export enum PoolType {
    RATED = 0,
    CASUAL = 1,
}

export interface PoolKey {
    poolType: PoolType;
    timeControl: TimeControlSettings;
}

export interface OpenSeek {
    userId: string;
    userName: string;
    pool: PoolKey;
    timeControl: TimeControl;
    rating?: number;
}

export type SeekKeyStr = `${PoolType}-${number}+${number}:${string}`;

export interface SeekKey {
    userId: string;
    pool: PoolKey;
}
