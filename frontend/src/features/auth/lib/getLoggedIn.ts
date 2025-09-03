import { getSessionUser, PrivateUser, SessionUser } from "@/lib/apiClient";
import constants from "@/lib/constants";
import { cookies } from "next/headers";
import { isAuthed } from "./userGuard";

export async function fetchUserSession(): Promise<{
    user: SessionUser;
    accessToken: string;
} | null> {
    const cookieStore = await cookies();
    const accessTokenCookie = cookieStore.get(constants.COOKIES.ACCESS_TOKEN);
    if (!accessTokenCookie) return null;

    const { data: user, error } = await getSessionUser({
        headers: { Cookie: cookieStore.toString() },
    });
    if (error || user === undefined) {
        console.error(error);
        return null;
    }

    return { user, accessToken: accessTokenCookie.value };
}

export async function fetchAuthedUserSession(): Promise<{
    user: PrivateUser;
    accessToken: string;
} | null> {
    const session = await fetchUserSession();
    if (!session) return null;

    const user = session.user;
    if (!isAuthed(user)) return null;

    return { user, accessToken: session.accessToken };
}
