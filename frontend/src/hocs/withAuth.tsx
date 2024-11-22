import { redirect } from "next/navigation";
import { cookies } from "next/headers";
import React from "react";

import AuthContextProvider from "../contexts/authContext";
import type { PrivateUser } from "@/lib/apiClient/models";
import { profileApi } from "@/lib/apiClient/client";

interface WithAuthProps extends JSX.IntrinsicAttributes {
    profile: PrivateUser;
}

/**
 * HOC to make sure the page is not accessible without the user being logged in.
 *
 * This HOC will send a request to `/profile/me/info-sensitive`, and if an unauthorized
 * HTTP status is returned the user will be redirected to the home page
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

        return (
            <AuthContextProvider hasAuthCookies={true} profile={profile}>
                <WrappedComponent {...props} profile={profile} />
            </AuthContextProvider>
        );
    };
    return NewComponent;
};
export default withAuth;
