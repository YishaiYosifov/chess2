"use client";

import { ReactNode, createContext, useRef } from "react";
import { type StoreApi } from "zustand";

import {
    type ChessboardStore,
    createChessboardStore,
} from "@/stores/chessboardStore";

export const ChessStoreContext =
    createContext<StoreApi<ChessboardStore> | null>(null);

export const ChessProvider = ({
    store,
    children,
}: {
    store: StoreApi<ChessboardStore>;
    children: ReactNode;
}) => {
    return (
        <ChessStoreContext.Provider value={store}>
            {children}
        </ChessStoreContext.Provider>
    );
};
