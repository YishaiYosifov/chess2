import { redirect } from "next/navigation";

import { profileApi } from "@/lib/apis";
import { ComponentType } from "react";
import AuthContextProvider from "../contexts/AuthContext";

/**
 * HOC to make sure the page is not accessible without the user being logged in.
 *
 * This HOC will send a request to `/profile/me/info-sensitive`, and if an unauthorized
 * HTTP status is returned the user will be redirected to the home page
 */
const withAuth = <T,>(WrappedComponent: ComponentType<T>) => {
    return async (props: any) => {
        try {
            const profile = await profileApi.getInfoSensitive(
                {},
                {
                    cache: "no-cache",
                }
            );
            return (
                <AuthContextProvider profile={profile}>
                    <WrappedComponent {...props} profile={profile} />
                </AuthContextProvider>
            );
        } catch {
            redirect("/");
        }
    };
};
export default withAuth;
