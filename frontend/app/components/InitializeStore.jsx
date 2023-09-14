"use client";

import { useRef } from "react";

import { useStore } from "../store";

const InitializeStore = ({ values }) => {
    const initialized = useRef(false);
    if (initialized.current) return;

    initialized.current = true;
    useStore.setState(values, false, "INITIALIZE_STORE");
};
export default InitializeStore;
