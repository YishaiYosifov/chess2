"use client";

import { signOut } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

const LogoutPage = () => {
    const router = useRouter();

    useEffect(() => {
        async function signOutAsync() {
            await signOut({ redirect: false });
            router.replace("/login");
        }
        signOutAsync();
    }, [router]);
};

export default LogoutPage;
