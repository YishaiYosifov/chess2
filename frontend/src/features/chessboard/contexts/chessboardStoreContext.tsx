"use client";

import { type StoreApi } from "zustand";
import { createContext } from "react";

import { type ChessboardStore } from "@/features/chessboard/stores/chessboardStore";

const ChessboardStoreContext = createContext<StoreApi<ChessboardStore> | null>(
    null,
);
export default ChessboardStoreContext;
