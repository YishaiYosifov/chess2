"use client";

import { ChallengeRequest } from "@/lib/apiClient";
import ChallengeFooter from "./ChallengeFooter";
import ChallengeHeader from "./ChallengeHeader";
import ChallengeStoreContext from "../../contexts/challengeContext";
import useConst from "@/hooks/useConst";
import { createChallengeStore } from "../../stores/challengeStore";
import ChallengeDescription from "./challengeDescriptions/ChallengeDescription";
import useChallengeEvents from "../../hooks/useChallengeEvents";

const ChallengeSidebar = ({ challenge }: { challenge: ChallengeRequest }) => {
    const challengeStore = useConst(() => createChallengeStore({ challenge }));
    useChallengeEvents(challengeStore, challenge.challengeToken);

    return (
        <ChallengeStoreContext.Provider value={challengeStore}>
            <aside className="flex h-full w-full min-w-xs flex-col gap-3 overflow-auto lg:max-w-sm">
                <ChallengeHeader />
                <ChallengeDescription />
                <ChallengeFooter />
            </aside>
        </ChallengeStoreContext.Provider>
    );
};
export default ChallengeSidebar;
