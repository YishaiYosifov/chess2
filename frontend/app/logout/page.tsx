"use client";

import { useRouter } from "next/navigation";

import { apiRequest } from "@/lib/utils/common";
import { setIsAuthed } from "@/zustand/store";

const LogoutPage = () => {
    const router = useRouter();
    apiRequest("/auth/logout").then(() => {
        setIsAuthed(false);
        router.replace("/");
    });
};

export default LogoutPage;
