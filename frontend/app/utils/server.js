import { cookies } from "next/headers";

import { apiRequest, processGetBody } from "./common";
import { updateProfile, useStore } from "../store";

export function getServerSession() {
    const headerCookies = cookies();
    if (!headerCookies.has("accessToken") || !headerCookies.has("refreshToken"))
        return {};

    const authValues = {
        isAuthed: true,
        accessToken: headerCookies.get("accessToken").value,
        refreshToken: headerCookies.get("refreshToken").value,
    };
    useStore.setState(authValues);
    return authValues;
}

/**
 * Initialize the session user in the store
 *
 * @param {Object} session - the session object
 * @param {string[]} include - attributes to include
 * @param {string[]} exclude - attributes to exclude
 * @returns {Object} - the user object
 */
export async function initializeSessionUserState({ include, exclude } = {}) {
    const response = await apiRequest(
        `/profile/me/info?${processGetBody({
            include: include || "",
            include_sensitive: true,
        })}`,
        {
            cache: "no-cache",
            method: "GET",
        }
    );
    const profile = await response.json();
    updateProfile(profile);
    return profile;
}
