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
    isExpired: boolean;
    cancelledBy: string | null;

    setCancelled(cancelledBy: string | null): void;
    setExpired(): void;
}

export function createChallengeStore(initState: ChallengeStoreProps) {
    return createWithEqualityFn<ChallengeStore>()(
        immer((set) => ({
            ...initState,
            isCancelled: false,
            isDeclined: false,
            isExpired: false,
            cancelledBy: null,

            setCancelled(cancelledBy) {
                set((state) => {
                    state.isCancelled = true;
                    state.cancelledBy = cancelledBy;
                });
            },
            setExpired() {
                set((state) => {
                    state.isExpired = true;
                });
            },
        })),
        shallow,
    );
}
