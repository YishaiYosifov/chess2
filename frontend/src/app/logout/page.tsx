"use client";

import { useRouter } from "next/navigation";

import { useAuthedContext } from "@/hooks/useAuthed";
import { authApi } from "@/lib/apis";

const LogoutPage = () => {
    const router = useRouter();
    const { setHasAuthCookies } = useAuthedContext();

    authApi.logout().then(() => {
        setHasAuthCookies(false);
        router.replace("/");
    });
};

export default LogoutPage;
