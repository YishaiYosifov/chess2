"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

import { createGuestUser } from "@/lib/apiClient";
import constants from "@/lib/constants";

/**
 * Create a guest user and retry
 */
const GuestRedirect = () => {
    const router = useRouter();

    useEffect(() => {
        async function handleCreateGuest() {
            const { error } = await createGuestUser();
            if (error) {
                router.replace(constants.PATHS.SIGNIN);
                return;
            }

            router.refresh();
        }
        handleCreateGuest();
    }, [router]);

    return null;
};
export default GuestRedirect;
