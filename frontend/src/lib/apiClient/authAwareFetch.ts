import ensureAuth from "@/features/auth/lib/ensureAuth";
import { navigate } from "@/actions/navigate";
import { logout } from "./definition";
import constants from "../constants";
import rawClient from "./rawClient";

export default async function authAwareFetch(
    input: URL | RequestInfo,
    init?: RequestInit,
): Promise<Response> {
    const response = await fetch(input, init);

    // if the server is making this request we don't want to auto refresh
    const isServerRequest = typeof window === "undefined";
    if (response.status !== 401 || isServerRequest) return response;

    const canRetryAuthed = await ensureAuth();
    if (!canRetryAuthed) {
        await handleLogout();
        return response;
    }

    const newResponse = await fetch(input, init);
    if (newResponse.status === 401) {
        await handleLogout();
        return newResponse;
    }

    return newResponse;
}

async function handleLogout(): Promise<void> {
    await logout({ client: rawClient });
    await navigate(constants.PATHS.SIGNIN);
}
