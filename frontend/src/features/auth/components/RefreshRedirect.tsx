"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

import { logout, refresh } from "@/lib/apiClient";
import constants from "@/lib/constants";

/**
 * This component is reposible for refreshing the user's access token
 * before reaching a server component.
 *
 * When the middleware detects we are logged in but don't have an access token,
 * we are redirected here, we refresh the token and redirect back to the desired page
 */
const RefreshRedirect = ({ redirectTo }: { redirectTo: string }) => {
    const router = useRouter();

    useEffect(() => {
        async function handleRefresh() {
            const { error } = await refresh();
            if (error) {
                await logout();
                router.replace(constants.PATHS.REGISTER);
                return;
            }

            router.replace(redirectTo);
        }
        handleRefresh();
    }, [redirectTo, router]);

    return null;
};
export default RefreshRedirect;
