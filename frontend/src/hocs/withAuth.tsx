import { redirect } from "next/navigation";
import { cookies } from "next/headers";
import React, { JSX } from "react";

import { getAuthedUser, type PrivateUser } from "@/lib/apiClient";
import constants from "@/lib/constants";
import RefreshProvider from "@/components/RefreshProvider";

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
        const cookieStore = await cookies();
        if (!cookieStore.has(constants.IS_AUTHED_COOKIE)) redirect("/login");
        const { data, error } = await getAuthedUser({
            headers: { Cookie: cookieStore.toString() },
        });

        if (error || !data) {
            console.error(error);
            return <RefreshProvider />;
        }

        return <WrappedComponent {...props} />;
    };
    return NewComponent;
};
export default withAuth;
