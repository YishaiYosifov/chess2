"use client";

import { ChallengeRequest } from "@/lib/apiClient";
import ChallengeFooter from "./ChallengeFooter";
import ChallengeHeader from "./ChallengeHeader";
import Card from "@/components/ui/Card";
import ChallengeStoreContext from "../../contexts/challengeContext";
import useConst from "@/hooks/useConst";
import { createChallengeStore } from "../../stores/challengeStore";
import ChallengeDescription from "./challengeDescriptions/ChallengeDescription";
import useChallengeEvents from "../../hooks/useChallengeEvents";

const ChallengeSidebar = ({ challenge }: { challenge: ChallengeRequest }) => {
    const challengeStore = useConst(() => createChallengeStore({ challenge }));
    useChallengeEvents(challengeStore, challenge.challengeId);

    return (
        <ChallengeStoreContext.Provider value={challengeStore}>
            <aside className="flex w-full min-w-xs flex-col gap-3 lg:max-w-sm">
                <ChallengeHeader />
                <ChallengeDescription />

                <Card className="items-center">
                    <ChallengeFooter />
                </Card>
            </aside>
        </ChallengeStoreContext.Provider>
    );
};
export default ChallengeSidebar;
