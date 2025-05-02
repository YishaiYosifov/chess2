"use client";

import { ReactNode, createContext, useState } from "react";
import { PrivateUser } from "@/lib/client/types.gen";

export interface AuthContextInterface {
    hasAuthCookies: boolean;
    setHasAuthCookies: (isAuthed: boolean) => void;
    setAuthedProfile: (profile: PrivateUser) => void;
    authedProfile?: PrivateUser;
}

export const AuthContext = createContext<AuthContextInterface>(
    {} as AuthContextInterface,
);

const AuthContextProvider = ({
    hasAuthCookies = false,
    profile,
    children,
}: {
    profile?: PrivateUser;
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
