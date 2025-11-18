"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

import { logout } from "@/lib/apiClient";
import constants from "@/lib/constants";

const LogoutPage = () => {
    const router = useRouter();

    useEffect(() => {
        async function handleLogOut() {
            await logout();
            router.replace(constants.PATHS.SIGNIN);
        }
        handleLogOut();
    }, [router]);
};

export default LogoutPage;
