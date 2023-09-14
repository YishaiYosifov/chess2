import { NextResponse } from "next/server";

import { apiRequest } from "./app/utils/common";

export async function middleware(request) {
    const isLoggedIn = request.cookies.has("refreshToken");
    if (request.cookies.has("accessToken") || !isLoggedIn)
        return NextResponse.next();

    const response = NextResponse.redirect(request.url);

    const refreshToken = request.cookies.get("refreshToken").value;
    const accessTokenResponse = await apiRequest("/auth/regen-access-token", {
        cache: "no-store",
        authToken: refreshToken,
        method: "GET",
    });
    const { accessToken, accessTokenMaxAge } = await accessTokenResponse.json();

    response.cookies.set("accessToken", accessToken, {
        maxAge: accessTokenMaxAge,
        httpOnly: true,
    });

    return response;
}
