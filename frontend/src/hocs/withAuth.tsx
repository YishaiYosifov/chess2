import { redirect } from "next/navigation";
import React, { JSX } from "react";

import type { PrivateUser } from "@/lib/apiClient";
import { auth } from "@/lib/auth";

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
        const session = await auth();
        if (!session) redirect("/login");

        return <WrappedComponent {...props} />;
    };
    return NewComponent;
};
export default withAuth;
