"use client";

import { ReactNode, createContext, useContext, useState } from "react";
import { PrivateUserOut } from "@/client";

interface AuthContext {
    setIsAuthed: (isAuthed: boolean) => void;
    setAuthedProfile: (profile: PrivateUserOut) => void;
    authedProfile: PrivateUserOut;
}

type ConditionalAuth =
    | ({ isAuthed: true } & AuthContext)
    | ({ isAuthed: false } & Partial<AuthContext>);

export const AuthContext = createContext<ConditionalAuth>({ isAuthed: false });

/**
 * Get the authed user context, or raise an error if not loaded
 *
 * @returns the auth context object
 */
export function useAuthedContext(): Required<ConditionalAuth> {
    const context = useContext(AuthContext);
    if (!context.isAuthed) throw Error("Profile Not Loaded");

    return context;
}

/**
 * Get the authed user profile, or raise an error if not loaded
 *
 * @returns the authed user profile
 */
export function useAuthedProfile(): PrivateUserOut {
    return useAuthedContext().authedProfile;
}

const AuthContextProvider = ({
    profile,
    children,
}: {
    profile: PrivateUserOut;
    children: ReactNode;
}) => {
    const [isAuthed, setIsAuthed] = useState(true);
    const [authedProfile, setAuthedProfile] = useState(profile);

    return (
        <AuthContext.Provider
            value={{
                isAuthed,
                setIsAuthed,
                setAuthedProfile,
                authedProfile,
            }}
        >
            {children}
        </AuthContext.Provider>
    );
};
export default AuthContextProvider;
