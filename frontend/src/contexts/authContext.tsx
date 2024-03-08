"use client";

import { ReactNode, createContext, useState } from "react";
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
