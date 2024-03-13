"use client";

import { ReactNode, createContext, useContext, useRef } from "react";
import { type StoreApi, useStore } from "zustand";

import { type ChessStore, createChessStore } from "@/stores/chessStore";
import { Piece } from "@/components/game/chess.types";

const ChessStoreContext = createContext<StoreApi<ChessStore> | null>(null);

export const ChessProvider = ({
    pieces,
    children,
}: {
    pieces: Map<string, Piece>;
    children: ReactNode;
}) => {
    const storeRef = useRef<StoreApi<ChessStore>>();
    if (!storeRef.current) storeRef.current = createChessStore({ pieces });

    return (
        <ChessStoreContext.Provider value={storeRef.current}>
            {children}
        </ChessStoreContext.Provider>
    );
};

export const useChessStore = <T,>(selector: (store: ChessStore) => T): T => {
    const counterStoreContext = useContext(ChessStoreContext);

    if (!counterStoreContext)
        throw new Error(`useCounterStore must be use within ChessProvider`);

    return useStore(counterStoreContext, selector);
};
