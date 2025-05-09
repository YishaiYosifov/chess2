"use client";

import { createContext, useState } from "react";
import { PrivateUser } from "@/lib/apiClient";

export interface AuthContextInterface {
    hasAccessToken: boolean;
    setHasAccessToken: (isAuthed: boolean) => void;
    setAuthedProfile: (profile: PrivateUser) => void;
    authedProfile?: PrivateUser;
}

export const AuthContext = createContext<AuthContextInterface>({
    hasAccessToken: false,
    setHasAccessToken: () => {},
    setAuthedProfile: () => {},
});

const AuthContextProvider = ({
    hasAccessToken = false,
    user,
    children,
}: {
    user?: PrivateUser;
    hasAccessToken?: boolean;
    children: React.ReactNode;
}) => {
    const [hasAccessTokenState, setHasAccessTokenState] =
        useState(hasAccessToken);
    const [authedProfile, setAuthedProfile] = useState(user);

    return (
        <AuthContext.Provider
            value={{
                hasAccessToken: hasAccessTokenState,
                setHasAccessToken: setHasAccessTokenState,
                setAuthedProfile,
                authedProfile,
            }}
        >
            {children}
        </AuthContext.Provider>
    );
};
export default AuthContextProvider;
