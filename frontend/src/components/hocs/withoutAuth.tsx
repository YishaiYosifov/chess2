import { redirect } from "next/navigation";
import { cookies } from "next/headers";

/**
 * HOC to make sure the page is not accessible when the user is logged in.
 *
 * If the user has an access / refresh token cookie they will be redirected to the home page.
 */
const withoutAuth = (WrappedComponent: any) => {
    return async (props: any) => {
        const nextCookies = cookies();
        if (nextCookies.has("access_token") || nextCookies.has("refresh_token"))
            redirect("/");

        return <WrappedComponent {...props} />;
    };
};
export default withoutAuth;
