import { StoreApi } from "zustand";
import { ChallengeStore } from "../stores/challengeStore";
import { createContext } from "react";

const ChallengeStoreContext = createContext<StoreApi<ChallengeStore> | null>(
    null,
);
export default ChallengeStoreContext;
