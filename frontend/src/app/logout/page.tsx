"use client";

import { useRouter } from "next/navigation";

import { setIsAuthed } from "@/zustand/store";
import { authApi } from "@/lib/apis";

const LogoutPage = () => {
    const router = useRouter();
    authApi.logout().then(() => {
        setIsAuthed(false);
        router.replace("/");
    });
};

export default LogoutPage;
