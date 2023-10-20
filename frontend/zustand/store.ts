import { createWithEqualityFn } from "zustand/traditional";
import { StoreApi, UseBoundStore } from "zustand";
import { devtools } from "zustand/middleware";
import { shallow } from "zustand/shallow";

import { AuthSlice, createAuthSlice } from "./slices/authSlice";

type WithSelectors<S> = S extends { getState: () => infer T }
    ? S & { use: { [K in keyof T]: () => T[K] } }
    : never;

/**
 * Generate the .use method of the store.
 * This is some ungodly looking typescript code I copied from the zustand docs
 */
export function createSelectors<S extends UseBoundStore<StoreApi<object>>>(
    _store: S
): WithSelectors<S> {
    let store = _store as WithSelectors<typeof _store>;
    store.use = {};
    for (let key of Object.keys(store.getState())) {
        (store.use as any)[key] = () => store((s) => s[key as keyof typeof s]);
    }

    return store;
}

export type State = AuthSlice;

export const useStore = createSelectors(
    createWithEqualityFn<State>()(
        devtools((...a) => ({
            ...createAuthSlice(...a),
        })),
        shallow
    )
);

/** Update the local logged in user profile */
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
