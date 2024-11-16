import { RequestContext, Configuration, AuthApi } from "@/lib/apiClient";

/**
 * Update the cookies when a request is sent from the server
 */
async function addServerCookies(
    context: RequestContext,
): Promise<RequestContext | void> {
    if (typeof window !== "undefined") return;

    const { cookies } = await import("next/headers");

    const headers = new Headers(context.init.headers);
    headers.set("Cookie", cookies().toString());
    context.init.headers = headers;

    return context;
}

export const apiConfig = new Configuration({
    basePath: "http://localhost:5116/api",
    credentials: "include",
    preRequest: addServerCookies,
});

export const authApi = new AuthApi(apiConfig);
