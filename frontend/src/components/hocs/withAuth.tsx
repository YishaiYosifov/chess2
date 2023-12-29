import { redirect } from "next/navigation";

import { profileApi } from "@/lib/apis";

/**
 * HOC to make sure the page is not accessible without the user being logged in.
 *
 * This HOC will send a request to `/profile/me/info`, and if an unauthorized HTTP status is returned
 * the user will be redirected to the home page
 */
const withAuth = (WrappedComponent: any) => {
    return async (props: any) => {
        try {
            const profile = await profileApi.getInfoSensitive({
                cache: "no-cache",
            });
            return <WrappedComponent {...props} profile={profile} />;
        } catch {
            redirect("/");
        }
    };
};
export default withAuth;
