import { StateCreator } from "zustand";
import type { Store } from "../store";

export interface AuthSlice {
    isAuthed: boolean;

    csrfToken: string;
    csrfTokenCreatedAt: number;
}

export const initialAuthState: AuthSlice = {
    isAuthed: false,

    csrfToken: "",
    csrfTokenCreatedAt: 0,
};

export const createAuthSlice: StateCreator<
    Store,
    [],
    [],
    AuthSlice
> = (): AuthSlice => ({
    ...initialAuthState,
});
