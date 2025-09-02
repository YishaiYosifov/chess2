import { redirect } from "next/navigation";
import React from "react";

import { Renderable, renderRenderable } from "@/lib/utils/renderable";
import SessionProvider from "../contexts/sessionContext";
import { fetchUserSession } from "../lib/getLoggedIn";
import { type SessionUser } from "@/lib/apiClient";
import constants from "@/lib/constants";

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
    if (!session) redirect(constants.PATHS.LOGOUT);

    return (
        <SessionProvider user={session.user} fetchAttempted>
            {renderRenderable(children, session)}
        </SessionProvider>
    );
}
