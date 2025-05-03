import { redirect } from "next/navigation";
import { cookies } from "next/headers";
import React, { JSX } from "react";

import { PrivateUser, getAuthedUser } from "@/lib/apiClient";
import AuthContextProvider from "../contexts/authContext";

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

        const { data, error } = await getAuthedUser({
            headers: { Cookie: nextCookies.toString() },
        });
        if (error || !data) {
            console.error("Could not find logged in user:", error);
            redirect("/login");
        }

        return (
            <AuthContextProvider hasAuthCookies={true} profile={data}>
                <WrappedComponent {...props} profile={data} />
            </AuthContextProvider>
        );
    };
    return NewComponent;
};
export default withAuth;
