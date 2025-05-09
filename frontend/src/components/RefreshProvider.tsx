"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

import { refresh } from "@/lib/apiClient";

const RefreshProvider = () => {
    const router = useRouter();
    useEffect(() => {
        async function refreshAsync() {
            await refresh();
            router.refresh();
        }
        refreshAsync();
    }, [router]);

    return <></>;
};
export default RefreshProvider;
