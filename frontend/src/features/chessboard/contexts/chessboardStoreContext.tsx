"use client";

import { ReactNode, createContext, useRef } from "react";
import { type StoreApi } from "zustand";

import {
    type ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";

export const ChessboardStoreContext =
    createContext<StoreApi<ChessboardStore> | null>(null);

export const ChessProvider = ({
    children,
    ...state
}: {
    children: ReactNode;
} & Partial<ChessboardStore>) => {
    const storeRef = useRef<StoreApi<ChessboardStore>>(null);
    if (!storeRef.current) storeRef.current = createChessboardStore(state);

    return (
        <ChessboardStoreContext.Provider value={storeRef.current}>
            {children}
        </ChessboardStoreContext.Provider>
    );
};
