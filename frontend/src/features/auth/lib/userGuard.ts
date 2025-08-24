import { GuestUser, PrivateUser, SessionUser } from "@/lib/apiClient";

export function isAuthed(user: SessionUser): user is PrivateUser {
    const type: PrivateUser["type"] = "authed";
    return user.type === type;
}

export function isGuest(user: SessionUser): user is GuestUser {
    const type: GuestUser["type"] = "guest";
    return user.type === type;
}
