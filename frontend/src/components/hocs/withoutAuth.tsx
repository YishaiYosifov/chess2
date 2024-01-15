import { redirect } from "next/navigation";
import { cookies } from "next/headers";
import { ComponentType } from "react";

import constants from "@/lib/constants";

/**
 * HOC to make sure the page is not accessible when the user is logged in.
 * If the user has an access / refresh token cookie they will be redirected to the home page.
 */
const withoutAuth = <T,>(WrappedComponent: ComponentType<T>) => {
    return async (props: any) => {
        const nextCookies = cookies();
        if (
            nextCookies.has(constants.ACCESS_TOKEN) ||
            nextCookies.has(constants.REFRESH_TOKEN)
        )
            redirect("/");

        return <WrappedComponent {...props} />;
    };
};
export default withoutAuth;
