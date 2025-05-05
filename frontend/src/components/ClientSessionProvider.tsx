"use client";

import { SessionProvider, signOut } from "next-auth/react";
import type { Session } from "next-auth";
import React, { useEffect } from "react";

const ClientSessionProvider = ({
    children,
    session,
}: {
    children: React.ReactNode;
    session: Session | null;
}) => {
    useEffect(() => {
        if (session && !session.userQuerySuccessful) signOut();
    }, [session]);

    return <SessionProvider session={session}>{children}</SessionProvider>;
};
export default ClientSessionProvider;
