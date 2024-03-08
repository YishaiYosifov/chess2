"use client";

import { ReactNode, createContext, useContext, useState } from "react";
import { PrivateAuthedProfileOut } from "@/client";

export interface AuthContextInterface {
    hasAuthCookies: boolean;
    setHasAuthCookies: (isAuthed: boolean) => void;
    setAuthedProfile: (profile: PrivateAuthedProfileOut) => void;
    authedProfile?: PrivateAuthedProfileOut;
}

export const AuthContext = createContext<AuthContextInterface>(
    {} as AuthContextInterface
);

/**
 * Get the authed user context, or raise an error if not loaded
 *
 * @returns the auth context object
 */

export function useAuthedContext(): Required<AuthContextInterface> {
    const context = useContext(AuthContext);
    if (!context.hasAuthCookies || context.authedProfile === undefined)
        throw Error("Profile Not Loaded");

    return context as Required<AuthContextInterface>;
}

/**
 * Get the authed user profile, or raise an error if not loaded
 *
 * @returns the authed user profile
 */

export function useAuthedProfile(): PrivateAuthedProfileOut {
    return useAuthedContext().authedProfile;
}

const AuthContextProvider = ({
    hasAuthCookies = false,
    profile,
    children,
}: {
    profile?: PrivateAuthedProfileOut;
    hasAuthCookies?: boolean;
    children: ReactNode;
}) => {
    const [hasAuthCookiesState, setHasAuthCookiesState] =
        useState(hasAuthCookies);
    const [authedProfile, setAuthedProfile] = useState(profile);

    return (
        <AuthContext.Provider
            value={{
                hasAuthCookies: hasAuthCookiesState,
                setHasAuthCookies: setHasAuthCookiesState,
                setAuthedProfile,
                authedProfile,
            }}
        >
            {children}
        </AuthContext.Provider>
    );
};
export default AuthContextProvider;
