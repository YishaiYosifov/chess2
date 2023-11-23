import { NextRequest, NextResponse } from "next/server";

import { apiRequest } from "@/lib/utils/common";

/**
 * Make sure the user has an access token if they have a refresh token
 */
export async function middleware(request: NextRequest): Promise<NextResponse> {
    const isLoggedIn = request.cookies.has("refresh_token_cookie");
    if (request.cookies.has("access_token_cookie") || !isLoggedIn)
        return NextResponse.next();

    const response = NextResponse.redirect(request.url);

    let accessTokenResponse: Response;
    try {
        accessTokenResponse = await apiRequest("/auth/refresh-access-token", {
            cache: "no-store",
            method: "GET",
        });
    } catch {
        response.cookies.delete("refresh_token");
        return response;
    }

    const {
        accessToken,
        accessTokenMaxAge,
    }: { accessToken: string; accessTokenMaxAge: number } = (
        await accessTokenResponse.json()
    ).data;

    response.cookies.set("access_token", accessToken, {
        maxAge: accessTokenMaxAge,
        httpOnly: true,
    });

    return response;
}
