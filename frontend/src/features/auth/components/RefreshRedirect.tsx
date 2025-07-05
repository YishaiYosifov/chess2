"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

import constants from "@/lib/constants";
import { logout, refresh } from "@/lib/apiClient";
import { ArrowPathIcon } from "@heroicons/react/24/solid";

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
                router.replace(constants.PATHS.LOGIN);
                return;
            }

            router.replace(redirectTo);
        }
        handleRefresh();
    }, [redirectTo, router]);

    return (
        <div className="flex h-screen justify-center text-white">
            <ArrowPathIcon
                className="w-32 animate-spin"
                data-testid="seekingSpinner"
            />
        </div>
    );
};
export default RefreshRedirect;
