import { redirect } from "next/navigation";
import { cookies } from "next/headers";
import React from "react";

import { getSessionUser, type SessionUser } from "@/lib/apiClient";
import constants from "@/lib/constants";
import SessionProvider from "../contexts/sessionContext";

interface WithAuthedSessionProps {
    user: SessionUser;
    accessToken: string;
}

/**
 * HOC to make sure the page is not accessible without the user having a valid access token,
 * whether it be guest or authed.
 */
const withSession = <P extends WithAuthedSessionProps>(
    WrappedComponent: React.ComponentType<P>,
) => {
    const NewComponent = async (props: P) => {
        const cookieStore = await cookies();
        const accessTokenCookie = cookieStore.get(
            constants.COOKIES.ACCESS_TOKEN,
        );
        if (!accessTokenCookie) redirect(constants.PATHS.LOGIN);

        const { data: user, error } = await getSessionUser({
            auth: () => accessTokenCookie.value,
        });
        if (error || !user) {
            console.error(error);
            redirect(constants.PATHS.LOGOUT);
        }

        return (
            <SessionProvider user={user}>
                <WrappedComponent
                    {...props}
                    user={user}
                    accessToken={accessTokenCookie.value}
                />
            </SessionProvider>
        );
    };
    return NewComponent;
};
export default withSession;
