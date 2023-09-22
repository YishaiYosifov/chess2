"use client";

import { ReactElement, useRef } from "react";

import { useStore, Store } from "@/app/store";

const InitializeStore = ({
    values,
}: {
    values: Partial<Store>;
}): ReactElement | undefined => {
    const initialized = useRef(false);
    if (initialized.current) return;

    initialized.current = true;
    useStore.setState(values, false, "INITIALIZE_STORE");
};
export default InitializeStore;
