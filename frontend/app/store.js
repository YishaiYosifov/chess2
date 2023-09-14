import { createWithEqualityFn } from "zustand/traditional";
import { devtools } from "zustand/middleware";

import { createAuthSlice } from "./slices/authSlice";
import { shallow } from "zustand/shallow";
import { produce } from "immer";

function createSelectors(store) {
    store.use = {};
    for (let key of Object.keys(store.getState()))
        store.use[key] = () => store((state) => state[key]);

    return store;
}

export const useStore = createSelectors(
    createWithEqualityFn(
        devtools((set, get) => ({
            profile: {},
            ...createAuthSlice(set, get),
        })),
        shallow
    )
);

export function updateProfile(profile) {
    useStore.setState(
        produce((state) => {
            Object.assign(state.profile, profile);
        }),
        false,
        "UPDATE_PROFILE"
    );
}
