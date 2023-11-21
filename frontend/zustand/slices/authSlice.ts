import { StateCreator } from "zustand";

import type { PublicProfile } from "@/lib/types";
import type { State } from "../store";

export const enum AuthMethods {
    Credentials,
    Email,
    Guest,
}

// The local profile has every field public profile has but also includes sensitive information
export interface LocalProfile extends Partial<PublicProfile> {
    email?: string;
    usernameLastChanged?: string;
}

export interface AuthSlice {
    csrfToken: string;
    csrfTokenCreatedAt: number;
    isAuthed: boolean;
    localProfile: LocalProfile;
}

// Initialize the slice
export const initialAuthState: AuthSlice = {
    csrfToken: "",
    csrfTokenCreatedAt: 0,

    isAuthed: false,
    localProfile: {},
};

export const createAuthSlice: StateCreator<
    State,
    [],
    [],
    AuthSlice
> = (): AuthSlice => ({
    ...initialAuthState,
});
