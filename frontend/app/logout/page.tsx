"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

import { logout } from "../store";

const LogoutPage = () => {
    const router = useRouter();

    useEffect(() => {
        (async () => {
            await logout();
            router.replace("/");
        })();
    }, []);
};

export default LogoutPage;
