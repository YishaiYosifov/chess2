import { NextRequest, NextResponse } from "next/server";
import { authApi } from "./lib/apis";

import constants from "./lib/constants";

/**
 * Handle access token refresh
 */
export async function middleware(request: NextRequest) {
    const accessToken = request.cookies.get(constants.ACCESS_TOKEN);
    const refreshToken = request.cookies.get(constants.REFRESH_TOKEN);
    if (!refreshToken || accessToken) return NextResponse.next();

    const response = NextResponse.redirect(request.url);
    try {
        const tokens = await authApi.refreshAccessTokenRaw({
            refreshToken: refreshToken?.value,
        });

        response.headers.set(
            "Set-Cookie",
            tokens.raw.headers.get("Set-Cookie")!
        );
    } catch {
        response.cookies.delete(constants.REFRESH_TOKEN);
        return response;
    }

    return response;
}
