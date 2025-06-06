import { redirect } from "next/navigation";
import { cookies } from "next/headers";
import React from "react";

import { getMyId } from "@/lib/apiClient";
import constants from "@/lib/constants";

interface WithAuthedSessionProps {
    userId: string;
}

/**
 * HOC to make sure the page is not accessible without the user having a valid access token,
 * whether it be guest or authed.
 */
const withAuthedSession = <P extends WithAuthedSessionProps>(
    WrappedComponent: React.ComponentType<P>,
) => {
    const NewComponent = async (props: P) => {
        const cookieStore = await cookies();
        if (!cookieStore.has(constants.COOKIES.ACCESS_TOKEN))
            redirect(constants.PATHS.LOGIN);

        const { data: userId, error } = await getMyId({
            headers: { Cookie: cookieStore.toString() },
        });
        if (error || !userId) {
            console.error(error);
            redirect(constants.PATHS.LOGOUT);
        }

        return <WrappedComponent {...props} userId={userId} />;
    };
    return NewComponent;
};
export default withAuthedSession;
