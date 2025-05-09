"use client";

import { useRouter } from "next/navigation";
import { useContext, useEffect } from "react";

import { logout } from "@/lib/apiClient";
import constants from "@/lib/constants";
import { AuthContext } from "@/contexts/authContext";

const LogoutPage = () => {
    const router = useRouter();
    const { setHasAccessToken } = useContext(AuthContext);

    useEffect(() => {
        async function handleLogOut() {
            await logout();
            setHasAccessToken(false);
            router.replace(constants.PATHS.LOGIN);
        }
        handleLogOut();
    }, [router, setHasAccessToken]);
};

export default LogoutPage;
