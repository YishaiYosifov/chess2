import { SessionUser } from "@/lib/apiClient";
import { immer } from "zustand/middleware/immer";
import { shallow } from "zustand/shallow";
import { createWithEqualityFn } from "zustand/traditional";

export interface SessionStoreProps {
    user: SessionUser | null;
    fetchAttempted: boolean;
}

export interface SessionStore {
    user: SessionUser | null;
    fetchAttempted: boolean;

    setUser(user: SessionUser): void;
    markFetchAttempted(): void;
}

export function createSessionStore(initState: SessionStoreProps) {
    return createWithEqualityFn<SessionStore>()(
        immer((set) => ({
            ...initState,

            setUser(user) {
                set((state) => {
                    state.user = user;
                    state.fetchAttempted = true;
                });
            },

            markFetchAttempted() {
                set((state) => {
                    state.fetchAttempted = true;
                });
            },
        })),
        shallow,
    );
}
