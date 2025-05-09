"use client";

import { createContext, useState } from "react";
import { PrivateUser } from "@/lib/apiClient";

export interface AuthContextInterface {
    hasAuthCookies: boolean;
    setHasAuthCookies: (isAuthed: boolean) => void;
    setAuthedProfile: (profile: PrivateUser) => void;
    authedProfile?: PrivateUser;
}

export const AuthContext = createContext<AuthContextInterface>({
    hasAuthCookies: false,
    setHasAuthCookies: () => {},
    setAuthedProfile: () => {},
});

const AuthContextProvider = ({
    hasAuthCookies = false,
    user,
    children,
}: {
    user?: PrivateUser;
    hasAuthCookies?: boolean;
    children: React.ReactNode;
}) => {
    const [hasAuthCookiesState, setHasAuthCookiesState] =
        useState(hasAuthCookies);
    const [authedProfile, setAuthedProfile] = useState(user);

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
