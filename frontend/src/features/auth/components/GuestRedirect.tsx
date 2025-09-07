"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

import { createGuestUser } from "@/lib/apiClient";
import constants from "@/lib/constants";

/**
 * Create a guest user and redirect
 */
const GuestRedirect = ({ redirectTo }: { redirectTo: string }) => {
    const router = useRouter();

    useEffect(() => {
        async function handleCreateGuest() {
            const { error } = await createGuestUser();
            if (error) {
                router.replace(constants.PATHS.REGISTER);
                return;
            }

            router.replace(redirectTo);
        }
        handleCreateGuest();
    }, [redirectTo, router]);

    return null;
};
export default GuestRedirect;
