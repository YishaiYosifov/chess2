import { ComponentType, ReactPropTypes } from "react";
import { redirect } from "next/navigation";

import type { LocalProfile } from "@/lib/slices/profileSlice";
import { apiRequest } from "@/lib/utils/common";
import { updateProfile } from "@/app/store";

import InitializeStore from "../InitializeStore";

/**
 * HOC to make sure the page is not accessible without the user being logged in.
 *
 * This HOC will send a request to `/profile/me/info`, and if an unauthorized HTTP status is returned
 * the user will be redirected to the home page
 */
const withAuth = (WrappedComponent: ComponentType<ReactPropTypes>) => {
    return async (props: ReactPropTypes) => {
        const response = await apiRequest(
            "/profile/me/info?include_sensitive=1",
            {
                cache: "no-cache",
                method: "GET",
            }
        );
        if (!response.ok) redirect("/");

        const profile: LocalProfile = await response.json();
        updateProfile(profile);
        return (
            <>
                <InitializeStore
                    values={{ profile: profile, isAuthed: true }}
                />
                <WrappedComponent {...props} />
            </>
        );
    };
};
export default withAuth;
