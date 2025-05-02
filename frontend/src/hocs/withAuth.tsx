import { redirect } from "next/navigation";
import { cookies } from "next/headers";
import React, { JSX } from "react";

import type { PrivateUser } from "@/lib/apiClient/models";
import AuthContextProvider from "../contexts/authContext";
import { profileApi } from "@/lib/client";

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
        let profile: PrivateUser;
        try {
            profile = await profileApi.getAuthedUser({
                headers: { Cookie: nextCookies.toString() },
            });
        } catch (err) {
            console.error("Could not find logged in user:", err);
            redirect("/login");
        }
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
