import {
    GameRequestsApi,
    RequestContext,
    Configuration,
    SettingsApi,
    LiveGameApi,
    ProfileApi,
    AuthApi,
} from "@/lib/apiClient";

/**
 * Update the cookies when a request is sent from the server
 */
async function addServerCookies(
    context: RequestContext
): Promise<RequestContext | void> {
    if (typeof window !== "undefined") return;

    const { cookies } = await import("next/headers");

    const headers = new Headers(context.init.headers);
    headers.set("Cookie", cookies().toString());
    context.init.headers = headers;

    return context;
}

export const apiConfig = new Configuration({
    basePath: process.env.NEXT_PUBLIC_API_URL,
    credentials: "include",
    preRequest: addServerCookies,
});

export const gameRequestApi = new GameRequestsApi(apiConfig);
export const liveGameApi = new LiveGameApi(apiConfig);
export const settingsApi = new SettingsApi(apiConfig);
export const profileApi = new ProfileApi(apiConfig);
export const authApi = new AuthApi(apiConfig);
