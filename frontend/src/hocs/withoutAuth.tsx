import { ComponentType, JSX } from "react";
import { redirect } from "next/navigation";
import { cookies } from "next/headers";

import constants from "@/lib/constants";

/**
 * HOC to make sure the page is not accessible when the user is logged in.
 * If the user has an access / refresh token cookie they will be redirected to the home page.
 */
const withoutAuth = <P extends JSX.IntrinsicAttributes>(
    WrappedComponent: ComponentType<P>,
) => {
    const NewComponent = async (props: P) => {
        const cookieStore = await cookies();
        if (cookieStore.has(constants.IS_AUTHED_COOKIE)) redirect("/");

        return <WrappedComponent {...props} />;
    };
    return NewComponent;
};
export default withoutAuth;
