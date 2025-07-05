import { createContext } from "react";
import { StoreApi } from "zustand";

import { LiveChessStore } from "../stores/liveChessboardStore";

const LiveChessStoreContext = createContext<StoreApi<LiveChessStore> | null>(
    null,
);
export default LiveChessStoreContext;
