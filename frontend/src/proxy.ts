import { NextRequest, NextResponse } from "next/server";

import constants from "@/lib/constants";

export async function proxy(request: NextRequest) {
    const hasAuthCookie = request.cookies.has(constants.COOKIES.ACCESS_TOKEN);
    const shouldBeAuthed = request.cookies.has(constants.COOKIES.IS_LOGGED_IN);

    // if the user doesn't have an auth cookie
    // but is expected to be authenticated, we should refresh the token
    if (!hasAuthCookie && shouldBeAuthed) {
        const response = rewriteTo(request, constants.PATHS.REFRESH);
        return response;
    }

    return NextResponse.next();
}

function rewriteTo(request: NextRequest, newPathname: string): NextResponse {
    const originalPathname = request.nextUrl.pathname;
    const url = request.nextUrl.clone();
    url.pathname = newPathname;

    const response = NextResponse.rewrite(url);
    response.headers.set(
        constants.HEADERS.REDIRECT_AFTER_AUTH,
        originalPathname,
    );
    return response;
}

export const config = {
    matcher: ["/((?!_next|favicon.ico|api).*)"], // Run on all paths except static assets and api routes
};
