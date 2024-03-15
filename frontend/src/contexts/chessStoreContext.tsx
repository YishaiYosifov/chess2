"use client";

import { ReactNode, createContext, useRef } from "react";
import { type StoreApi } from "zustand";

import { type ChessStore, createChessStore } from "@/stores/chessStore";

export const ChessStoreContext = createContext<StoreApi<ChessStore> | null>(
    null
);

export const ChessProvider = ({
    children,
    ...state
}: {
    children: ReactNode;
} & Partial<ChessStore>) => {
    const storeRef = useRef<StoreApi<ChessStore>>();
    if (!storeRef.current) storeRef.current = createChessStore(state);

    return (
        <ChessStoreContext.Provider value={storeRef.current}>
            {children}
        </ChessStoreContext.Provider>
    );
};
