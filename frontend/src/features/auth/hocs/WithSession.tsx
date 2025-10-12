import React from "react";

import { Renderable, renderRenderable } from "@/lib/utils/renderable";
import SessionProvider from "../contexts/sessionContext";
import { fetchUserSession } from "../lib/getLoggedIn";
import { type SessionUser } from "@/lib/apiClient";
import GuestRedirect from "../components/GuestRedirect";

interface WithAuthedSessionProps {
    user: SessionUser;
    accessToken: string;
}

export default async function WithSession({
    children,
}: {
    children: Renderable<WithAuthedSessionProps>;
}) {
    const session = await fetchUserSession();
    if (!session) return <GuestRedirect />;

    return (
        <SessionProvider user={session.user} fetchAttempted>
            {renderRenderable(children, session)}
        </SessionProvider>
    );
}
