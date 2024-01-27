"use client";

import { useRouter } from "next/navigation";

import { authApi } from "@/lib/apis";
import { useAuthedContext } from "@/components/contexts/AuthContext";

const LogoutPage = () => {
    const router = useRouter();
    const { setIsAuthed } = useAuthedContext();

    authApi.logout().then(() => {
        setIsAuthed(false);
        router.replace("/");
    });
};

export default LogoutPage;
