import { ComponentType, ReactPropTypes } from "react";
import { redirect } from "next/navigation";

import type { LocalProfile } from "@/app/slices/profileSlice";
import { useStore, updateProfile } from "@/app/store";
import { apiRequest } from "@/lib/utils/common";

import InitializeStore from "../InitializeStore";

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
