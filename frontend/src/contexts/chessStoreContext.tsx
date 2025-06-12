"use client";

import { ReactNode, createContext, useRef } from "react";
import { type StoreApi } from "zustand";

import { type ChessStore, createChessStore } from "@/stores/chessStore";

export const ChessStoreContext = createContext<StoreApi<ChessStore> | null>(
    null,
);

export const ChessProvider = ({
    store,
    children,
}: {
    store: StoreApi<ChessStore>;
    children: ReactNode;
}) => {
    return (
        <ChessStoreContext.Provider value={store}>
            {children}
        </ChessStoreContext.Provider>
    );
};
