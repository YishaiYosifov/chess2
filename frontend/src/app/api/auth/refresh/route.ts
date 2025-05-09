import { NextRequest, NextResponse } from "next/server";

import constants from "@/lib/constants";
import { refresh } from "@/lib/apiClient";

export async function GET(request: NextRequest) {
    const refreshToken = request.cookies.get(constants.COOKIES.REFRESH_TOKEN);
    if (!refreshToken) return Response.redirect(constants.PATHS.LOGIN);

    try {
        const { response: refreshResponse, error } = await refresh({
            headers: { Cookie: `${refreshToken.name}=${refreshToken.value}` },
        });

        if (error) return Response.redirect(constants.PATHS.LOGIN);

        const redirectTo =
            request.headers.get(constants.HEADERS.REFRESH_REDIRECT) ?? "/";
        const response = NextResponse.redirect(
            new URL(redirectTo, request.url),
        );
        for (const cookie of refreshResponse.headers.getSetCookie()) {
            response.headers.append("Set-Cookie", cookie);
        }

        return response;
    } catch (error) {
        console.error("Error refreshing token:", error);
        return Response.redirect(constants.PATHS.LOGIN);
    }
}
