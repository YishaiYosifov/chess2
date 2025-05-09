"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

import { logout } from "@/lib/apiClient";

const LogoutPage = () => {
    const router = useRouter();

    useEffect(() => {
        async function handleLogOut() {
            await logout();
            router.replace("/login");
        }
        handleLogOut();
    }, [router]);
};

export default LogoutPage;
