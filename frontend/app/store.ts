import { createWithEqualityFn } from "zustand/traditional";
import { StoreApi, UseBoundStore } from "zustand";
import { devtools } from "zustand/middleware";
import { shallow } from "zustand/shallow";
import { produce } from "immer";

import {
    LocalProfile,
    ProfileSlice,
    createProfileSlice,
} from "./slices/profileSlice";
import {
    AuthSlice,
    createAuthSlice,
    initialAuthState,
} from "./slices/authSlice";
import { apiRequest } from "@/lib/utils/common";

type WithSelectors<S> = S extends { getState: () => infer T }
    ? S & { use: { [K in keyof T]: () => T[K] } }
    : never;

function createSelectors<S extends UseBoundStore<StoreApi<object>>>(_store: S) {
    let store = _store as WithSelectors<typeof _store>;
    store.use = {};
    for (let key of Object.keys(store.getState())) {
        (store.use as any)[key] = () => store((s) => s[key as keyof typeof s]);
    }

    return store;
}

export type Store = AuthSlice & ProfileSlice;

export const useStore = createSelectors(
    createWithEqualityFn<Store>()(
        devtools((...a) => ({
            ...createAuthSlice(...a),
            ...createProfileSlice(...a),
        })),
        shallow
    )
);

export function updateProfile(profile: LocalProfile): void {
    useStore.setState(
        produce((state: Store): void => {
            Object.assign(state.profile, profile);
        }),
        false,
        "UPDATE_PROFILE"
    );
}

export function setCsrfToken(csrf: string): void {
    useStore.setState(
        {
            csrfToken: csrf,
            csrfTokenCreatedAt: new Date().valueOf() / 1000,
        },
        false,
        "SET_CSRF"
    );
}

export function setIsAuthed(isAuthed: boolean): void {
    useStore.setState({ isAuthed }, false, "LOGIN");
}

export async function logout(): Promise<void> {
    await apiRequest("/auth/logout");
    useStore.setState(initialAuthState, false, "LOGOUT");
}
