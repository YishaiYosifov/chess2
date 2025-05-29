"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

import constants from "@/lib/constants";
import { createGuestUser } from "@/lib/apiClient";

/**
 * Create a guest user and redirect
 */
const Guest = ({ redirectTo }: { redirectTo: string }) => {
    const router = useRouter();

    useEffect(() => {
        async function handleCreateGuest() {
            const { error } = await createGuestUser();
            if (error) {
                router.replace(constants.PATHS.LOGIN);
                return;
            }

            router.replace(redirectTo);
        }
        handleCreateGuest();
    }, [redirectTo, router]);

    return <></>;
};
export default Guest;
