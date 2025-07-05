import { redirect } from "next/navigation";
import { cookies } from "next/headers";
import React from "react";

import { getMyId } from "@/lib/apiClient";
import constants from "@/lib/constants";

interface WithAuthedSessionProps {
    userId: string;
    accessToken?: string;
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
        const accessTokenCookie = cookieStore.get(
            constants.COOKIES.ACCESS_TOKEN,
        );
        if (!accessTokenCookie) redirect(constants.PATHS.LOGIN);

        const { data: userId, error } = await getMyId({
            auth: () => accessTokenCookie.value,
        });
        if (error || !userId) {
            console.error(error);
            redirect(constants.PATHS.LOGOUT);
        }

        return (
            <WrappedComponent
                {...props}
                userId={userId}
                accessToken={accessTokenCookie.value}
            />
        );
    };
    return NewComponent;
};
export default withAuthedSession;
