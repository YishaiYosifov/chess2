import { GuestUser, PrivateUser, SessionUser } from "@/lib/apiClient";

export function isAuthed(user: SessionUser | null): user is PrivateUser {
    if (user === null) return false;

    const type: PrivateUser["type"] = "authed";
    return user.type === type;
}

export function isGuest(user: SessionUser | null): user is GuestUser {
    if (user === null) return false;

    const type: GuestUser["type"] = "guest";
    return user.type === type;
}
