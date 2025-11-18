"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

import { logout, refresh } from "@/lib/apiClient";
import constants from "@/lib/constants";

/**
 * Refresh access token and retry
 */
const RefreshRedirect = () => {
    const router = useRouter();

    useEffect(() => {
        async function handleRefresh() {
            const { error } = await refresh();
            if (error) {
                await logout();
                router.replace(constants.PATHS.SIGNIN);
                return;
            }

            router.refresh();
        }
        handleRefresh();
    }, [router]);

    return null;
};
export default RefreshRedirect;
