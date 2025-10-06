import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";

import { ChallengeRequest } from "@/lib/apiClient";

export interface ChallengeStoreProps {
    challenge: ChallengeRequest;
}

export interface ChallengeStore {
    challenge: ChallengeRequest;
    isCancelled: boolean;
    hasExpired: boolean;
    isExpired: boolean;

    setCancelled(): void;
    setExpired(): void;
}

export function createChallengeStore(initState: ChallengeStoreProps) {
    return createWithEqualityFn<ChallengeStore>()(
        immer((set) => ({
            ...initState,
            isCancelled: false,
            hasExpired: false,
            isExpired: false,

            setCancelled() {
                set((state) => {
                    state.isCancelled = true;
                });
            },
            setExpired() {
                set((state) => {
                    state.hasExpired = true;
                    state.isExpired = true;
                });
            },
        })),
        shallow,
    );
}
