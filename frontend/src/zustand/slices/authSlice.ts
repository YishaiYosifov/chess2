import { StateCreator } from "zustand";

import { PrivateUserOut } from "@/client";
import type { State } from "../store";

export interface AuthSlice {
    csrfToken: string;
    csrfTokenCreatedAt: number;
    isAuthed: boolean;
    localProfile: PrivateUserOut | null;
}

// Initialize the slice
export const initialAuthState: AuthSlice = {
    csrfToken: "",
    csrfTokenCreatedAt: 0,

    isAuthed: false,
    localProfile: null,
};

export const createAuthSlice: StateCreator<
    State,
    [],
    [],
    AuthSlice
> = (): AuthSlice => ({
    ...initialAuthState,
});
