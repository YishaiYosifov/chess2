import constants from "@/lib/constants";
import { createGuestUser, refresh } from "@/lib/apiClient/definition";
import rawClient from "@/lib/apiClient/rawClient";
import Cookies from "js-cookie";

let authPromise: Promise<boolean> | null = null;
export default async function ensureAuth(): Promise<boolean> {
    if (authPromise) return authPromise;

    authPromise = createSession();
    authPromise.finally(() => (authPromise = null));
    return authPromise;
}

async function createSession(): Promise<boolean> {
    if (Cookies.get(constants.COOKIES.IS_LOGGED_IN)) {
        const didRefresh = await handleRefresh();
        return didRefresh;
    } else {
        const didCreateGuest = await handleGuest();
        return didCreateGuest;
    }
}

async function handleRefresh(): Promise<boolean> {
    const { error } = await refresh({ client: rawClient });
    if (!error) return true;

    console.error("Failed refreshing:", error);
    return false;
}

async function handleGuest(): Promise<boolean> {
    const { error } = await createGuestUser({ client: rawClient });
    if (!error) return true;

    console.error("Failed creating guest:", error);
    return false;
}
