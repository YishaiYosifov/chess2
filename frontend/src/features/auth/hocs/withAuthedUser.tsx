import { redirect } from "next/navigation";
import { cookies } from "next/headers";
import React from "react";

import SessionContextProvider from "@/features/auth/contexts/sessionContext";
import { getSessionUser, type PublicUser } from "@/lib/apiClient";
import constants from "@/lib/constants";
import { isAuthed } from "../lib/userGuard";

interface WithAuthedUserProps {
    user: PublicUser;
}

/**
 * HOC to make sure the page is not accessible without the user being logged in.
 *
 * This HOC will send a request to check if we are authorized, and if not
 * the user will be redirected to the login page
 */
const withAuthedUser = <P extends WithAuthedUserProps>(
    WrappedComponent: React.ComponentType<P>,
) => {
    const NewComponent = async (props: P) => {
        const cookieStore = await cookies();
        if (!cookieStore.has(constants.COOKIES.ACCESS_TOKEN))
            redirect(constants.PATHS.REGISTER);

        const { data: user, error } = await getSessionUser({
            headers: { Cookie: cookieStore.toString() },
        });
        if (error || !user) {
            console.error(error);
            redirect(constants.PATHS.LOGOUT);
        }

        if (!isAuthed(user)) redirect(constants.PATHS.REGISTER);

        return (
            <SessionContextProvider user={user}>
                <WrappedComponent {...props} user={user} />
            </SessionContextProvider>
        );
    };
    return NewComponent;
};
export default withAuthedUser;
