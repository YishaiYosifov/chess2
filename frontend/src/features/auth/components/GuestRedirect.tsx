"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

import constants from "@/lib/constants";
import { createGuestUser } from "@/lib/apiClient";
import { ArrowPathIcon } from "@heroicons/react/24/solid";

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

    return (
        <div className="flex h-screen justify-center text-white">
            <ArrowPathIcon
                className="w-32 animate-spin"
                data-testid="seekingSpinner"
            />
        </div>
    );
};
export default GuestRedirect;
