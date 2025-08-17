import { RequestCookies } from "next/dist/compiled/@edge-runtime/cookies";
import { NextURL } from "next/dist/server/web/next-url";
import { NextRequest, NextResponse } from "next/server";

import { middleware } from "../middleware";
import constants from "@/lib/constants";

vi.mock("next/server", () => ({
    NextRequest: vi.fn(),
    NextResponse: {
        next: vi.fn(() => ({ type: "next" })),
        rewrite: vi.fn((url) => ({ type: "rewrite", url, headers: new Map() })),
    },
}));

describe("middleware", () => {
    function createRequest({
        setCookies,
        pathname,
    }: {
        setCookies: Record<string, string>;
        pathname?: string;
    }): NextRequest {
        pathname ??= "/some-path";
        const url = new URL(`http://localhost:3000${pathname}`);
        const nextUrl = new NextURL(url);
        const headers = new Headers();
        const cookies = new RequestCookies(headers);
        for (const [name, value] of Object.entries(setCookies)) {
            cookies.set(name, value);
        }

        const request = {
            nextUrl,
            cookies,
            headers,
        } as unknown as NextRequest;
        return request;
    }

    it.each([
        {
            [constants.COOKIES.ACCESS_TOKEN]: "access",
            [constants.COOKIES.IS_LOGGED_IN]: "true",
        },
        {
            [constants.COOKIES.ACCESS_TOKEN]: "access",
        },
    ])(
        "should not attempt to refresh when not needed",
        async (setCookies: Record<string, string>) => {
            const request = createRequest({ setCookies });

            const response = await middleware(request);

            expect(NextResponse.next).toHaveBeenCalled();
            expect(response).toEqual({ type: "next" });
        },
    );

    it.each([
        [
            {
                [constants.COOKIES.IS_LOGGED_IN]: "true",
            },
            constants.PATHS.REFRESH,
        ],
        [{}, constants.PATHS.GUEST],
    ])(
        "should rewrite to refresh path if needed",
        async (setCookies: Record<string, string>, rewriteTo: string) => {
            const pathname = "/some-path";
            const request = createRequest({
                setCookies,
                pathname,
            });

            const response = await middleware(request);

            expect(NextResponse.rewrite).toHaveBeenCalled();
            expect(response.type).toBe("rewrite");

            const url = new URL(response.url);
            expect(url.pathname).toBe(rewriteTo);
            expect(
                response.headers.get(constants.HEADERS.REDIRECT_AFTER_AUTH),
            ).toBe(pathname);
        },
    );
});
