import { redirect } from "next/navigation";

import type { LocalProfile } from "@/zustand/slices/authSlice";
import { apiRequest } from "@/lib/utils/common";

/**
 * HOC to make sure the page is not accessible without the user being logged in.
 *
 * This HOC will send a request to `/profile/me/info`, and if an unauthorized HTTP status is returned
 * the user will be redirected to the home page
 */
const withAuth = (WrappedComponent: any) => {
    return async (props: any) => {
        const response = await apiRequest(
            "/profile/me/info?include_sensitive=1",
            {
                cache: "no-cache",
                method: "GET",
            }
        );
        if (!response.ok) redirect("/");

        const profile: LocalProfile = (await response.json()).data;
        return <WrappedComponent {...props} profile={profile} />;
    };
};
export default withAuth;
