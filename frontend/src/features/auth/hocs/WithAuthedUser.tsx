import { redirect } from "next/navigation";
import React from "react";

import SessionProvider from "@/features/auth/contexts/sessionContext";
import { Renderable, renderRenderable } from "@/lib/utils/renderable";
import { fetchAuthedUserSession } from "../lib/getLoggedIn";
import { type PrivateUser } from "@/lib/apiClient";
import constants from "@/lib/constants";

interface WithAuthedUserProps {
    user: PrivateUser;
    accessToken: string;
}

export default async function WithAuthedUser({
    children,
}: {
    children: Renderable<WithAuthedUserProps>;
}) {
    const session = await fetchAuthedUserSession();
    if (!session) redirect(constants.PATHS.LOGOUT);

    return (
        <SessionProvider user={session.user} fetchAttempted>
            {renderRenderable(children, session)}
        </SessionProvider>
    );
}
