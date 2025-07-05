import { createContext } from "react";
import { StoreApi } from "zustand";

import { LiveChessStore } from "../stores/liveChessboardStore";

export const LiveChessStoreContext =
    createContext<StoreApi<LiveChessStore> | null>(null);
