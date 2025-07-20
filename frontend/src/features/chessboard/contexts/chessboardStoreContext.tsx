"use client";

import { type StoreApi } from "zustand";
import { createContext } from "react";

import { type ChessboardState } from "@/features/chessboard/stores/chessboardStore";

const ChessboardStoreContext = createContext<StoreApi<ChessboardState> | null>(
    null,
);
export default ChessboardStoreContext;
