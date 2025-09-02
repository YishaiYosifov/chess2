"use client";

import { createContext } from "react";

import { createSessionStore, SessionStore } from "../stores/sessionStore";
import { SessionUser } from "@/lib/apiClient";
import { StoreApi } from "zustand";

export const SessionContext = createContext<StoreApi<SessionStore> | null>(
    null,
);

const SessionProvider = ({
    user,
    fetchAttempted,
    children,
}: {
    user: SessionUser | null;
    fetchAttempted?: boolean;
    children: React.ReactNode;
}) => {
    const store = createSessionStore({
        user,
        fetchAttempted: fetchAttempted || user !== null,
    });
    return (
        <SessionContext.Provider value={store}>
            {children}
        </SessionContext.Provider>
    );
};
export default SessionProvider;
