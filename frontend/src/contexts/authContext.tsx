"use client";

import { createContext } from "react";

import { PrivateUser } from "@/lib/apiClient";

export interface AuthContextInterface {
    user?: PrivateUser;
}

export const AuthContext = createContext<AuthContextInterface>({});

const AuthContextProvider = ({
    user,
    children,
}: {
    user: PrivateUser;
    children: React.ReactNode;
}) => {
    return (
        <AuthContext.Provider value={{ user }}>{children}</AuthContext.Provider>
    );
};
export default AuthContextProvider;
