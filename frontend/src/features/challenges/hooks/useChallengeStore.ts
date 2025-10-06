import { useContext } from "react";
import { ChallengeStore } from "../stores/challengeStore";
import ChallengeStoreContext from "../contexts/challengeContext";
import { useShallow } from "zustand/shallow";
import { useStore } from "zustand";

export default function useChallengeStore<T>(
    selector: (store: ChallengeStore) => T,
): T {
    const chessStoreContext = useContext(ChallengeStoreContext);
    if (!chessStoreContext)
        throw new Error(
            "useChallengeStore must be use within LiveChessStoreProvider",
        );

    return useStore(chessStoreContext, useShallow(selector));
}
