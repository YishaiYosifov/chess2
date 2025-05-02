import { redirect } from "next/navigation";
import { cookies } from "next/headers";
import React, { JSX } from "react";

import AuthContextProvider from "../contexts/authContext";
import { PrivateUser, getAuthedUser } from "@/lib/apiClient";

interface WithAuthProps extends JSX.IntrinsicAttributes {
    profile: PrivateUser;
}

/**
 * HOC to make sure the page is not accessible without the user being logged in.
 *
 * This HOC will send a request to `check if we are authorized, and if not
 * the user will be redirected to the home page
 */
const withAuth = <P extends WithAuthProps>(
    WrappedComponent: React.ComponentType<P>,
) => {
    const NewComponent = async (props: P) => {
        const nextCookies = await cookies();

        const { response, error } = await getAuthedUser({
            headers: { Cookie: nextCookies.toString() },
        });
        if (error || !response) {
            console.error("Could not find logged in user:", error);
            redirect("/login");
        }

        const profile = response as PrivateUser;
        console.log(profile);

        return (
            <AuthContextProvider hasAuthCookies={true} profile={profile}>
                <WrappedComponent {...props} profile={profile} />
            </AuthContextProvider>
        );
    };
    return NewComponent;
};
export default withAuth;
