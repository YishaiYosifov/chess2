import { createContext } from "react";
import { StoreApi } from "zustand";

import { LiveChessStore } from "../stores/liveChessStore";

const LiveChessStoreContext = createContext<StoreApi<LiveChessStore> | null>(
    null,
);
export default LiveChessStoreContext;
