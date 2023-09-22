import { ComponentType, ReactPropTypes } from "react";
import { redirect } from "next/navigation";

import InitializeStore from "../InitializeStore";
import { cookies } from "next/headers";

const withoutAuth = (WrappedComponent: ComponentType<ReactPropTypes>) => {
    return async (props: ReactPropTypes) => {
        const nextCookies = cookies();
        if (
            nextCookies.has("access_token_cookie") ||
            nextCookies.has("refresh_token_cookie")
        )
            redirect("/");

        return (
            <>
                <InitializeStore values={{ isAuthed: false }} />
                <WrappedComponent {...props} />
            </>
        );
    };
};
export default withoutAuth;
