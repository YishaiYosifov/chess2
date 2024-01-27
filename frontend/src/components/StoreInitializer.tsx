"use client";

import { ReactNode, useRef } from "react";

import { useStore, State } from "@/zustand/store";

const StoreInitializer = ({
    values,
    action,
    children,
}: {
    values: Partial<State>;
    action: string;
    children?: ReactNode;
}) => {
    const initialized = useRef(false);
    if (initialized.current) return children;

    useStore.setState(values, false, action);
    initialized.current = true;
    return children;
};

export default StoreInitializer;
