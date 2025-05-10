import { NextRequest, NextResponse } from "next/server";

import constants from "@/lib/constants";

export async function middleware(request: NextRequest) {
    const hasAuthCookie = request.cookies.has(constants.COOKIES.ACCESS_TOKEN);
    const shouldBeAuthed = request.cookies.has(constants.COOKIES.IS_AUTHED);

    // if we have an access token, no need to refresh
    // if the user is not authed, we can't refresh
    if (hasAuthCookie || !shouldBeAuthed) return NextResponse.next();

    const originalPathname = request.nextUrl.pathname;
    const url = request.nextUrl.clone();
    url.pathname = constants.PATHS.REFRESH;

    const response = NextResponse.rewrite(url);
    response.headers.set(constants.HEADERS.REFRESH_REDIRECT, originalPathname);
    return response;
}

export const config = {
    matcher: ["/((?!_next|favicon.ico).*)"], // Run on all paths except static assets
};
