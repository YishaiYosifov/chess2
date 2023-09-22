import { StateCreator } from "zustand";

import type { Store } from "../store";

export const enum AuthMethods {
    Credentials,
    Email,
    Guest,
}

export interface LocalProfile {
    userId?: number;
    authMethod?: AuthMethods;
    username?: string;
    email?: string;

    about?: string;
    country?: string;

    usernameLastChanged?: string;
    emailLastChanged?: string;
    pfpLastChanged?: string;
}

export interface ProfileSlice {
    profile: LocalProfile;
}

export const createProfileSlice: StateCreator<
    Store,
    [],
    [],
    ProfileSlice
> = (): ProfileSlice => ({
    profile: {},
});
