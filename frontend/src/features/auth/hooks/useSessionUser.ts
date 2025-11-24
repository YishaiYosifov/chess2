import { useContext, useEffect } from "react";
import { useShallow } from "zustand/shallow";
import { useStore } from "zustand";

import { getSessionUser, PrivateUser, SessionUser } from "@/lib/apiClient";
import { SessionContext } from "@/features/auth/contexts/sessionContext";
import { SessionStore } from "../stores/sessionStore";
import { isAuthed } from "../lib/userGuard";

export function useSessionStore<T>(selector: (store: SessionStore) => T): T {
    const sessionStoreContext = useContext(SessionContext);
    if (!sessionStoreContext)
        throw new Error("useSessionStore must be use within SessionProvider");

    return useStore(sessionStoreContext, useShallow(selector));
}

export function useSessionUser(): SessionUser | null {
    const { user, fetchAttempted, setUser, markFetchAttempted } =
        useSessionStore((x) => ({
            user: x.user,
            fetchAttempted: x.fetchAttempted,
            setUser: x.setUser,
            markFetchAttempted: x.markFetchAttempted,
        }));

    useEffect(() => {
        if (user || fetchAttempted) return;

        async function loadUser() {
            markFetchAttempted();

            const { error, data: loadedUser } = await getSessionUser();
            if (error || loadedUser === undefined) {
                console.error(error);
                return;
            }

            setUser(loadedUser);
        }
        loadUser();
    }, [user, fetchAttempted, setUser, markFetchAttempted]);

    return user;
}

export function useAuthedUser(): PrivateUser | null {
    const user = useSessionUser();

    if (!user || !isAuthed(user)) return null;
    return user;
}
