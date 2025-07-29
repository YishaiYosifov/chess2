import { useContext, useEffect } from "react";

import { SessionContext } from "@/features/auth/contexts/sessionContext";
import { getSessionUser, PrivateUser, SessionUser } from "@/lib/apiClient";
import { SessionStore } from "../stores/sessionStore";
import { useStore } from "zustand";
import { isAuthed } from "../lib/userGuard";

export function useSessionStore<T>(selector: (store: SessionStore) => T): T {
    const sessionStoreContext = useContext(SessionContext);
    if (!sessionStoreContext)
        throw new Error(
            "useSessionStore must be use within AuthContextProvider",
        );

    return useStore(sessionStoreContext, selector);
}

export function useSessionUser(): SessionUser | null {
    const user = useSessionStore((x) => x.user);
    const setUser = useSessionStore((x) => x.setUser);

    useEffect(() => {
        if (user) return;

        async function loadUser() {
            console.log("loadUser");
            const { error, data: loadedUser } = await getSessionUser();
            if (error || !loadedUser) {
                console.error(error);
                return;
            }

            setUser(loadedUser);
        }
        loadUser();
    }, [user, setUser]);

    return user;
}

export function useAuthedUser(): PrivateUser | null {
    const user = useSessionUser();

    if (!user || !isAuthed(user)) return null;
    return user;
}
