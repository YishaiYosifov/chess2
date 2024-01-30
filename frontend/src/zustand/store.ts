// Not currently used, will be used later in the project

import { createWithEqualityFn } from "zustand/traditional";
import { StoreApi, UseBoundStore } from "zustand";
import { devtools } from "zustand/middleware";
import { shallow } from "zustand/shallow";

type WithSelectors<S> = S extends { getState: () => infer T }
    ? S & { use: { [K in keyof T]: () => T[K] } }
    : never;

/**
 * Generate the .use method of the store
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

export type State = {};

export const useStore = createSelectors(
    createWithEqualityFn<State>()(
        devtools((...a) => ({})),
        shallow
    )
);
