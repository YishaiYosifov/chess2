import { StateCreator } from "zustand";

import type { LocalProfile } from "@/lib/types";
import type { State } from "../store";

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
