"use client";

import { useRef } from "react";

import { useStore, State } from "@/zustand/store";

const StoreInitializer = ({
    values,
    action,
}: {
    values: Partial<State>;
    action: string;
}) => {
    const initialized = useRef(false);
    if (initialized.current) return;

    useStore.setState(values, false, action);
    initialized.current = true;
    return null;
};

export default StoreInitializer;
