import { logout, refresh } from "./definition";
import { navigate } from "@/actions/navigate";
import constants from "../constants";
import rawClient from "./rawClient";

let refreshPromise: Promise<boolean> | null = null;
export default async function handleRefresh(): Promise<boolean> {
    if (refreshPromise) return refreshPromise;

    refreshPromise = (async () => {
        const { error } = await refresh({ client: rawClient });
        if (!error) return true;

        console.error("Failed refreshing:", error);
        await logout();
        await navigate(constants.PATHS.REGISTER);
        return false;
    })();
    refreshPromise.finally(() => (refreshPromise = null));

    return refreshPromise;
}
