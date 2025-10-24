import { useContext } from "react";
import LiveChessStoreContext from "../contexts/liveChessContext";
import { LiveChessStore } from "../stores/liveChessStore";
import { useStore } from "zustand";
import { useShallow } from "zustand/shallow";

export default function useLiveChessStore<T>(
    selector: (store: LiveChessStore) => T,
): T {
    const chessStoreContext = useContext(LiveChessStoreContext);

    if (!chessStoreContext)
        throw new Error(
            "useLiveChessStore must be use within LiveChessStoreProvider",
        );

    return useStore(chessStoreContext, useShallow(selector));
}
