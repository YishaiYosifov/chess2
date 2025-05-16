import { redirect } from "next/navigation";
import { cookies } from "next/headers";
import React from "react";

import { getAuthedUser, type PrivateUser } from "@/lib/apiClient";
import constants from "@/lib/constants";
import AuthContextProvider from "@/contexts/authContext";

interface WithAuthProps {
    user: PrivateUser;
}

/**
 * HOC to make sure the page is not accessible without the user being logged in.
 *
 * This HOC will send a request to check if we are authorized, and if not
 * the user will be redirected to the home page
 */
const withAuth = <P extends WithAuthProps>(
    WrappedComponent: React.ComponentType<P>,
) => {
    const NewComponent = async (props: P) => {
        const cookieStore = await cookies();
        if (!cookieStore.has(constants.COOKIES.ACCESS_TOKEN))
            redirect(constants.PATHS.LOGIN);

        const { data: user, error } = await getAuthedUser({
            headers: { Cookie: cookieStore.toString() },
        });
        if (error || !user) {
            console.error(error);
            redirect(constants.PATHS.LOGOUT);
        }

        return (
            <AuthContextProvider user={user}>
                <WrappedComponent {...props} user={user} />
            </AuthContextProvider>
        );
    };
    return NewComponent;
};
export default withAuth;
