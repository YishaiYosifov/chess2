"use client";

import { useRouter } from "next/navigation";

import { authApi } from "@/lib/apis";
import { useAuthedContext } from "@/components/contexts/AuthContext";

const LogoutPage = () => {
    const router = useRouter();
    const { setHasAuthCookies } = useAuthedContext();

    authApi.logout().then(() => {
        setHasAuthCookies(false);
        router.replace("/");
    });
};

export default LogoutPage;
