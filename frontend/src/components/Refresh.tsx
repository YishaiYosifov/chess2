"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

import constants from "@/lib/constants";
import { logout, refresh } from "@/lib/apiClient";

/**
 * This component is reposible for refreshing the user's access token
 * before reaching a server component.
 *
 * When the middleware detects we are logged in but don't have an access token,
 * we are redirected here, we refresh the token and redirect back to the desired page
 */
const Refresh = ({ redirectTo }: { redirectTo: string }) => {
    const router = useRouter();

    useEffect(() => {
        async function handleRefresh() {
            const { error } = await refresh();
            if (error) {
                await logout();
                router.replace(constants.PATHS.LOGIN);
                return;
            }

            router.replace(redirectTo);
        }
        handleRefresh();
    }, [redirectTo, router]);

    return <></>;
};
export default Refresh;
