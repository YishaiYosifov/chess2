import { cookies } from "next/headers";
import Cookies from "js-cookie";
import { Mock, MockedObject } from "vitest";

export function mockNextCookies(...cookieNames: string[]): Mock {
    const cookiesMock = cookies as Mock;
    cookiesMock.mockImplementation(() => ({
        has: (cookieName: string) => cookieNames.includes(cookieName),
    }));

    return cookiesMock;
}

export function mockJsCookie(
    cookies: Record<string, string | undefined>,
): MockedObject<typeof Cookies> {
    const cookiesMock = vi.mocked(Cookies);
    cookiesMock.get.mockImplementation(
        ((name?: string) => cookies[name ?? ""]) as typeof Cookies.get,
    );
    return cookiesMock;
}
