import { GuestUser, PublicUser, SessionUser } from "@/lib/apiClient";

export function isAuthed(user: SessionUser): user is PublicUser {
    const type: PublicUser["type"] = "authed";
    return user.type === type;
}

export function isGuest(user: SessionUser): user is GuestUser {
    const type: GuestUser["type"] = "guest";
    return user.type === type;
}
